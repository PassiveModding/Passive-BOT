using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PassiveBOT.Handlers;

namespace PassiveBOT.Discord.Extensions
{
    public class EventTriggers
    {
        public static async Task _client_UserJoined(SocketGuildUser User)
        {
            var DB = DatabaseHandler.GetGuild(User.Guild.Id);
            if (DB.Events.Welcome.Enabled)
            {
                var welcomeembed = new EmbedBuilder
                {
                    Title = $"Welcome to {User.Guild.Name}, {User}",
                    Description = $"{DB.Events.Welcome.Message}",
                    Color = Color.Green
                };

                if (DB.Events.Welcome.UserCount)
                {
                    welcomeembed.Footer = new EmbedFooterBuilder
                    {
                        Text = $"Users: {User.Guild.MemberCount}"
                    };
                }

                if (DB.Events.Welcome.SendDMs)
                {
                    try
                    {
                        await User.SendMessageAsync(User.Mention, false, welcomeembed.Build());
                    }
                    catch
                    {
                        //
                    }
                }

                if (User.Guild.GetChannel(DB.Events.Welcome.ChannelID) is ITextChannel WChannel)
                {
                    try
                    {
                        await WChannel.SendMessageAsync(User.Mention, false, welcomeembed.Build());
                    }
                    catch
                    {
                        //
                    }
                }
            }
        }

        public static async Task _client_UserLeft(SocketGuildUser User)
        {
            var DB = DatabaseHandler.GetGuild(User.Guild.Id);
            if (DB.Events.Goodbye.Enabled)
            {
                var GoodbyeEmbed = new EmbedBuilder
                {
                    Title = $"{User} has left the server",
                    Description = $"{DB.Events.Goodbye.Message}"
                };

                if (User.Guild.GetChannel(DB.Events.Goodbye.ChannelID) is ITextChannel GChannel)
                {
                    try
                    {
                        await GChannel.SendMessageAsync(User.Mention, false, GoodbyeEmbed.Build());
                    }
                    catch
                    {
                        //
                    }
                }
            }
        }
    }
}