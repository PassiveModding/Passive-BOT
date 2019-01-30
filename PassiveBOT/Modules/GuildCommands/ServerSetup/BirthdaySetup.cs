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
    using Discord.Commands;
    using Discord.WebSocket;

    using PassiveBOT.Context;
    using PassiveBOT.Models;

    public class BirthdaySetup : Base
    {
        public BirthdaySetup(BirthdayService bService)
        {
            Service = bService;
        }

        public BirthdayService Service { get; set; }

        [Command("EnableBirthdays")]
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

        [Command("RemoveBirthday")]
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

        [Command("SetBirthday")]
        public Task SetRole([Remainder]string dateTime = null)
        {
            if (Service.Model.Persons.Any(x => x.UserId == Context.User.Id))
            {
                return SimpleEmbedAsync("Sorry, your birthday has already been set. Please contact an administrator to change it,");
            }

            var yearTimeFormats = new[] { "dd MMM yyyy", "MMM dd yyyy", "dd/MMM/yyyy", "MMM/dd/yyyy", "dd-MMM-yyyy", "MMM-dd-yyyy" };
            //var noYearTimeFormats = new[] { "dd MMM", "MMM dd", "dd/MMM", "MMM/dd", "dd-MMM", "MMM-dd" };
            DateTime? parsedTime = null;
            if (DateTime.TryParseExact(dateTime, yearTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime resultWithYear))
            {
                parsedTime = resultWithYear;
            } 
            else if (DateTime.TryParseExact(dateTime, "dd MMM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime resultWithoutYear))
            {
                parsedTime = new DateTime(0001, resultWithoutYear.Month, resultWithoutYear.Day);
            }
            else
            {
                return SimpleEmbedAsync("Unable to retrieve a valid date format. Please use the following example: `01 Jan 2000` or `05 Feb`");
            }

            if (parsedTime.Value.Year == 0001)
            {
                Service.Model.AddBirthday(Context.User.Id, parsedTime.Value, false);
            }
            else
            {
                Service.Model.AddBirthday(Context.User.Id, parsedTime.Value, true);
            }

            Service.Save();
            return SimpleEmbedAsync($"Birthday set to {parsedTime.Value.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(parsedTime.Value.Month)} {(parsedTime.Value.Year == 0001 ? "" : parsedTime.Value.Year.ToString())}");
        }
    }
}
