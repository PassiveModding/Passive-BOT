using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Addons.Preconditions;
    using Discord.Commands;
    using Discord.WebSocket;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Models;

    public class BirthdaySetup : Base
    {
        public BirthdaySetup(BirthdayService bService)
        {
            Service = bService;
        }

        public BirthdayService Service { get; set; }

        [Command("EnableBirthdays")]
        [Summary("Enables birthday announcements in the current server")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public Task EnableGuild(IRole birthdayRole = null)
        {
            if (birthdayRole == null)
            {
                return SimpleEmbedAsync("Please provide a birthday role for birthdays to be enabled");
            }

            var guildMatch = Service.Model.EnabledGuilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if (guildMatch != null)
            {
                guildMatch.BirthdayChannelId = Context.Channel.Id;
                guildMatch.BirthdayRole = birthdayRole.Id;
                Service.Save();
                return SimpleEmbedAsync($"Birthday alerts will be sent to {Context.Channel.Name}");
            }

            Service.Model.EnabledGuilds.Add(new BirthdayService.BirthdayModel.EnabledGuild { BirthdayChannelId = Context.Channel.Id, GuildId = Context.Guild.Id, BirthdayRole = birthdayRole.Id});
            Service.Save();

            return SimpleEmbedAsync($"Birthday alerts will be sent to {Context.Channel.Name}");
        }

        [Command("DisableBirthdays")]
        [Summary("Disables birthday announcements for the current server")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task DisableGuild()
        {
            var guildMatch = Service.Model.EnabledGuilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if (guildMatch != null)
            {
                Service.Model.EnabledGuilds.Remove(guildMatch);
                Service.Save();
                return SimpleEmbedAsync($"Birthday alerts have been disabled.");
            }

            return SimpleEmbedAsync($"Birthday alerts were already disabled. No action has been taken");
        }

        [Command("SetBirthdayRole")]
        [Summary("Changes the birthday role set to automatically be given to users")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task SetRole(IRole role = null)
        {
            var guildMatch = Service.Model.EnabledGuilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if (guildMatch != null)
            {
                if (role != null)
                {
                    guildMatch.BirthdayRole = role.Id;
                    Service.Save();
                    return SimpleEmbedAsync($"Birthday role has been set to {role.Mention}");
                }

                guildMatch.BirthdayRole = 0;
                Service.Save();
                return SimpleEmbedAsync("Birthday role has removed.");
            }

            return SimpleEmbedAsync("You must run the enable birthdays command before setting a birthday role");
        }

        [Command("BirthdayList")]
        [Summary("Displays the birthdays of all users in the server")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(1, 15, Measure.Seconds)]
        public async Task GetBirthdayList()
        {
            try
            {
                var guildMatch = Service.Model.EnabledGuilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                if (guildMatch != null)
                {
                    var userMatches = Service.Model.Persons.Select(x => (Context.Guild.GetUser(x.UserId), x)).Where(x => x.Item1 != null).ToList();
                    if (userMatches.Any())
                    {
                        var firstUsers = userMatches.Where(x => x.Item2.IsToday()).ToList();
                        var userMatchesWithoutFirst = userMatches.Where(x => !x.Item2.IsToday()).OrderBy(x => x.Item2.Birthday.DayOfYear).ToList();

                        var pages = new List<PaginatedMessage.Page>();
                        var today = DateTime.Today;
                        if (firstUsers.Any())
                        {
                            pages.Add(new PaginatedMessage.Page { Title = $"Today's Birthdays {today.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(today.Month)}", Description = string.Join("\n", firstUsers.Select(x => x.Item2.ShowYear ? $"{x.Item1.Mention} || Age: {x.Item2.Age()}" : $"{x.Item1.Mention}")) });
                        }

                        foreach (var userGroup in userMatchesWithoutFirst.SplitList(10))
                        {
                            pages.Add(new PaginatedMessage.Page { Title = "Birthdays", Description = string.Join("\n", userGroup.Select(x => $"{DateAsDayAndMonth(x.Item2.Birthday)}: {x.Item1.Mention}" + (x.Item2.ShowYear ? $"({x.Item2.Age()})" : ""))) });
                        }
                        
                        var pager = new PaginatedMessage
                                        {
                                            Pages = pages,
                                            Color = Color.Blue
                                        };

                        await PagedReplyAsync(pager, new ReactionList { Forward = true, Backward = true, First = true, Last = true, Trash = true });
                        return;
                    }

                    await SimpleEmbedAsync("There are no users in this server with configured birthdays");
                    return;
                }

                await SimpleEmbedAsync("This server has not enabled birthdays");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await SimpleEmbedAsync("Error");
            }
        }

        public string DateAsDayAndMonth(DateTime dTime)
        {
            return $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dTime.Month)} {dTime.Day}";
        }

        [Command("RemoveBirthday")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        [Summary("Removes the current birthday from the given user")]
        public Task RemoveBirthday(SocketGuildUser user)
        {
            var person = Service.Model.Persons.FirstOrDefault(x => x.UserId == user.Id);
            if (person != null)
            {
                Service.Model.Persons.Remove(person);
                Service.Save();

                return SimpleEmbedAsync("Birthday has been removed!");
            }

            return SimpleEmbedAsync("User has never set a birthday and therefore cannot have theirs removed.");
        }

        private string[] dayTypes = { "d", "dd" };

        private string[] monthTypes = { "MMM", "MMMM" };

        private string[] delimiters = { " ", "-", "/" };

        public string[] getTimeFormats(bool useYear)
        {
            var responses = new List<string>();
            foreach (var dayType in dayTypes)
            {
                var baseFormat = dayType;
                foreach (var monthType in monthTypes)
                {
                    var secondaryFormat = $"{baseFormat} {monthType}";
                    if (useYear)
                    {
                        secondaryFormat += $" yyyy";
                    }

                    foreach (var delimiter in delimiters)
                    {
                        var delimited = secondaryFormat.Replace(" ", delimiter);
                        responses.Add(delimited);
                    }
                }
            }

            return responses.ToArray();
        }

        [Command("SetBirthday")]
        [Alias("Birthday")]
        [Summary("Set your birthday.")]
        public Task SetRole([Remainder]string dateTime = null)
        {
            if (Service.Model.Persons.Any(x => x.UserId == Context.User.Id))
            {
                return SimpleEmbedAsync("Sorry, your birthday has already been set. Please contact an administrator to change it.");
            }
            
            DateTime? parsedTime;
            if (DateTime.TryParseExact(dateTime, getTimeFormats(true), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime resultWithYear))
            {
                parsedTime = resultWithYear;
            } 
            else if (DateTime.TryParseExact(dateTime, getTimeFormats(false), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime resultWithoutYear))
            {
                parsedTime = new DateTime(0001, resultWithoutYear.Month, resultWithoutYear.Day);
            }
            else
            {
                return SimpleEmbedAsync("Unable to retrieve a valid date format. Please use the following example: `01 Jan 2000` or `05 Feb`");
            }

            Service.Model.AddBirthday(Context.User.Id, parsedTime.Value, parsedTime.Value.Year != 0001);

            Service.Save();
            return SimpleEmbedAsync($"Birthday set to {parsedTime.Value.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(parsedTime.Value.Month)} {(parsedTime.Value.Year == 0001 ? "" : parsedTime.Value.Year.ToString())}");
        }
    }
}
