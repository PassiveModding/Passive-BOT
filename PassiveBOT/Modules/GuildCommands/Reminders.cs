using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Modules.GuildCommands
{
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Services;

    public class Reminders : Base
    {
        private readonly ReminderService Remind;

        public Reminders(ReminderService remind)
        {
            Remind = remind;
        }

        [Command("Remind")]
        public Task RemindMe(TimeSpan span, [Remainder] string reminder)
        {
            if (span.TotalMinutes < 1)
            {
                return ReplyAsync("Reminders must be longer than 1 minute");
            }

            var response = Remind.AddReminder(Context.Guild?.Id ?? Context.User.Id, Context.User.Id, Context.Channel?.Id ?? Context.User.Id, reminder, span);

            return SimpleEmbedAsync("You will be reminded about: \n" + 
                                    $"{response.Message}\n" + 
                                    $"At: **{response.ExpiresOn.DateTime.ToShortTimeString()} {response.ExpiresOn.DateTime.ToShortDateString()}**\n" + 
                                    $"ID: {response.Id}");
        }
        
        [Command("Reminders")]
        public async Task RemindList()
        {
            var reminders = Remind.GetReminders(Context.User.Id);

            if (reminders.Any())
            {
                var reminderStrings = reminders.Select(
                    x =>
                        {
                            var timeString = $"@{x.ExpiresOn.DateTime.ToShortTimeString()} {x.ExpiresOn.DateTime.ToShortDateString()}";
                            var message = x.Message;
                            var guild = Context.Client.GetGuild(x.GuildId);
                            var channel = guild?.GetTextChannel(x.ChannelId);


                            if (channel != null)
                            {
                                var info = Format.Bold($"{guild.Name}/{channel.Name}/{timeString}");
                                return $"[**{x.Id}**][{info}] {message}";
                            }

                            return null;
                        });

                await SimpleEmbedAsync($"{string.Join("\n", reminderStrings.Where(x => x != null)).FixLength(2047)}");
            }
            else
            {
                await SimpleEmbedAsync("You do not have any reminders set");
            }
        }

        [Command("Delete Reminder")]
        public async Task DelReminder(int reminderId)
        {
            var res = Remind.DelReminder(Context.User.Id, reminderId);

            if (res)
            {
                await SimpleEmbedAsync("Reminder deleted");
            }
            else
            {
                await SimpleEmbedAsync("No reminder found with that ID");
            }
        }
    }
}
