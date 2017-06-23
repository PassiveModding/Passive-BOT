using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class GuildSetup : ModuleBase
    {
        [Command("Setup")]
        public async Task Setup()
        {
            GuildConfig.Setup(Context.Guild.Id, Context.Guild.Name);
            await ReplyAsync(GuildConfig.Read(Context.Guild.Id));
        }

        [Command("Welcome")]
        public async Task Welcome([Remainder] string message)
        {
            GuildConfig.SetWMessage(Context.Guild.Id, message);
            await ReplyAsync("The Welcome Message for this server has been set to:\n" +
                             $"**{message}**");
        }

        [Command("WelcomeChannel")]
        public async Task Wchannel()
        {
            GuildConfig.SetWChannel(Context.Guild.Id, Context.Channel.Id);
            await ReplyAsync("The Welcome Channel for this server has been set to:\n" +
                             $"**{Context.Channel.Name}**");
        }

        [Command("WelcomeStatus")]
        public async Task WOff(bool status)
        {
            GuildConfig.SetWelcomeStatus(Context.Guild.Id, status);
            await ReplyAsync($"Welcome Messageing for this server has been set to: {status}");
        }
    }
}