using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api.Extensions.DiscordNet;
using Newtonsoft.Json;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands.OwnerCmds
{
    [RequireOwner]
    public class Owner : InteractiveBase
    {
        public readonly CommandService Service;
        //public DiscordSocketClient Client;


        public Owner(CommandService Cserv)
        {
            Service = Cserv;
        }

        [Command("UpdateStats+")]
        [Summary("UpdateStats+")]
        [Remarks("Update the Bots Stats on DiscordBots.org")]
        public async Task UpdateStats()
        {
            if (Tokens.Load().DiscordBotsListToken == null)
            {
                await ReplyAsync("Bot Not Configured for DiscordBots.org");
                return;
            }

            try
            {
                var DblApi = new DiscordNetDblApi(Context.Client, Tokens.Load().DiscordBotsListToken);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(Context.Client.Guilds.Count);
            }
            catch
            {
                //
            }
        }

        [Command("PartnerUpdates+")]
        [Summary("PartnerUpdates+")]
        [Remarks("Set the PartnerUpdatesChannel")]
        public async Task PupDates()
        {
            var home = Homeserver.Load();
            home.PartnerUpdates = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("PartnerUpdates will now be posted here!");
        }

        [Command("SetBotModerators+")]
        [Summary("SetBotModerators+")]
        [Remarks("Set a role of users which can access some Bot Owner Commands")]
        public async Task BModRole(SocketRole role = null)
        {
            var home = Homeserver.Load();
            home.BotModerator = role?.Id ?? 0;
            Homeserver.SaveHome(home);
            await ReplyAsync($"ModRole is set to {(role == null ? "N/A" : role.Name)}!");
        }

        [Command("GlobalBan+", RunMode = RunMode.Async)]
        [Summary("GlobalBan+")]
        [Remarks("For those who dont seem to go away")]
        public async Task GlobalBan(ulong ID)
        {
            foreach (var server in Context.Client.Guilds)
                if (server.Users.Any(x => x.Id == ID))
                    try
                    {
                        await server.AddBanAsync(ID);
                        await ReplyAsync($"Banned User in {server.Name}");
                    }
                    catch
                    {
                        await ReplyAsync($"Failed to Ban User in {server.Name}");
                    }

            await ReplyAsync("Complete");
        }

        [Command("PurgeServers+", RunMode = RunMode.Async)]
        [Summary("PurgeServers+")]
        [Remarks("Delete old server configs")]
        public async Task DeleteServerConfigs()
        {
            await ReplyAsync("Working....");
            var purged = 0;
            foreach (var config in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "setup/server/")))
            {
                var p = Path.GetFileNameWithoutExtension(config);
                //Console.WriteLine(p);
                try
                {
                    var trythis = Context.Client.GetGuild(Convert.ToUInt64(p));
                    Console.WriteLine(trythis.Name);
                }
                catch
                {
                    File.Delete(config);
                    purged++;
                }
            }

            await ReplyAsync("Guilds Purged.\n" +
                             $"Purged: {purged}");
        }


        [Command("sethome+", RunMode = RunMode.Async)]
        [Summary("sethome+")]
        [Remarks("set the owner server")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task HomeAsync()
        {
            var homes = new Homeserver
            {
                GuildId = Context.Guild.Id,
                GuildName = Context.Guild.Name
            };
            Homeserver.SaveHome(homes);
            await ReplyAsync("Done");
        }

        [Command("setsuggest+")]
        [Summary("setsuggest+")]
        [Remarks("set the suggestion channel")]
        public async Task Suggest()
        {
            var home = Homeserver.Load();
            home.Suggestion = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("Done");
        }

        [Command("seterror+")]
        [Summary("seterror+")]
        [Remarks("set the suggestion channel")]
        public async Task Error()
        {
            var home = Homeserver.Load();
            home.Error = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("Done");
        }

        [Command("help+", RunMode = RunMode.Async)]
        [Summary("help+")]
        [Remarks("Owner Commands")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task Help2Async()
        {
            var description = "";
            foreach (var module in Service.Modules)
                if (module.Name == "Owner")
                    description = module.Commands.Aggregate(description,
                        (current, cmd) => current + $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n");

            var embed = new EmbedBuilder()
                .WithTitle("Owner Commands")
                .WithDescription(description);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("die+")]
        [Summary("die+")]
        [Remarks("Kills the bot")]
        public async Task Die()
        {
            await ReplyAsync("Bye Bye :heart:");
            Environment.Exit(0);
        }

        [Command("LeaveServer+", RunMode = RunMode.Async)]
        [Summary("LeaveServer+ <guild ID> [Optional]<reason>")]
        [Remarks("Makes the bot leave the specified guild")]
        public async Task LeaveAsync(ulong id, [Remainder] string reason = "No reason provided by the owner.")
        {
            if (id <= 0)
                await ReplyAsync("Please enter a valid Guild ID");
            var gld = Context.Client.GetGuild(id);
            //var ch = await gld.GetDefaultChannelAsync();
            foreach (var channel in gld.TextChannels)
                try
                {
                    await channel.SendMessageAsync($"Goodbye. `{reason}`");
                    break;
                }
                catch
                {
                    //
                }

            //await Task.Delay(500);
            await gld.LeaveAsync();
            await ReplyAsync("Message has been sent and I've left the guild!");
        }

        [Command("ReduceServers+", RunMode = RunMode.Async)]
        [Summary("ReduceServers+")]
        [Remarks("Reduce the amount of servers the bot is in.")]
        public async Task ReduceAsync()
        {
            var i = 0;
            await ReplyAsync("Leaving all servers with less than 15 members.");
            foreach (var guild in Context.Client.Guilds)
                if (guild.MemberCount < 15)
                {
                    await LeaveAsync(guild.Id,
                        "PassiveBOT is leaving this server due to low usercount. Please feel free to invite it back by going to our dev server and using the invite command:\n" +
                        $"{Tokens.Load().SupportServer}");
                    i++;
                }

            await ReplyAsync($"{i} servers left.");
        }

        [Command("GetServer+")]
        [Summary("Getserver+ <string>")]
        [Remarks("Get servers containing the privided string")]
        public async Task GetAsync([Remainder] string s)
        {
            var s2 = "";
            foreach (var guild in Context.Client.Guilds)
                if (guild.Name.ToLower().Contains(s.ToLower()))
                    s2 += $"{guild.Name} : {guild.Id}\n";
            if (s2 != "")
                await ReplyAsync(s2);
            else
                await ReplyAsync("No Servers containing the provided string available.");
        }

        [Command("Username+")]
        [Summary("username+ <name>")]
        [Remarks("Sets the bots username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                await ReplyAsync("Value cannot be empty");
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync("Bot Username updated").ConfigureAwait(false);
        }

        [Group("Token")]
        [RequireOwner]
        public class TokenSetup : ModuleBase
        {
            [Command("SetFortniteToken+")]
            [Summary("SetFortniteToken+ <token>")]
            [Remarks("set the fortnite api token")]
            public async Task FNToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.FortniteToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDialogFlowToken+")]
            [Summary("SetDialogFlowToken+")]
            [Remarks("set the DialogFlow api token")]
            public async Task DFToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DialogFlowToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDiscordBotsListToken+")]
            [Summary("SetDiscordBotsListToken+")]
            [Remarks("set the DBL api token")]
            public async Task DBLToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DiscordBotsListToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetTwitchToken+")]
            [Summary("SetTwitchToken+")]
            [Remarks("set the Twitch api token")]
            public async Task TwitchToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.TwitchToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDiscordBotsListURL+")]
            [Summary("SetDiscordBotsListURL+")]
            [Remarks("set the DBL bot URL")]
            public async Task DBLUrl([Remainder] string URL = null)
            {
                if (URL == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DiscordBotsListUrl = URL;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetSupportServerURL+")]
            [Summary("SetSupportServerURL+")]
            [Remarks("set the DBL bot URL")]
            public async Task SupportURL([Remainder] string URL = null)
            {
                if (URL == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.SupportServer = URL;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }
        }
    }
}