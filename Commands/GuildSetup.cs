using System;
using System.IO;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class GuildSetup : ModuleBase
    {
        [Command("Setup")]
        [Summary("Setup")]
        [Remarks("Initialises the servers configuration file")]
        public async Task Setup()
        {
            GuildConfig.Setup(Context.Guild.Id, Context.Guild.Name);
            await ReplyAsync(GuildConfig.Read(Context.Guild.Id));
        }

        [Command("Config")]
        [Summary("Config")]
        [Remarks("Shows the servers current configuration")]
        public async Task Config()
        {
            await ReplyAsync(GuildConfig.Read(Context.Guild.Id));
        }

        [Command("Welcome")]
        [Summary("Welcome <message>")]
        [Remarks("Sets the welcome message for new users in the server")]
        public async Task Welcome([Remainder] string message)
        {
            GuildConfig.SetWMessage(Context.Guild.Id, message);
            await ReplyAsync("The Welcome Message for this server has been set to:\n" +
                             $"**{message}**");
        }

        [Command("WelcomeChannel")]
        [Alias("wc")]
        [Summary("wc")]
        [Remarks("Sets the current channel as the welcome channel")]
        public async Task Wchannel()
        {
            GuildConfig.SetWChannel(Context.Guild.Id, Context.Channel.Id);
            await ReplyAsync("The Welcome Channel for this server has been set to:\n" +
                             $"**{Context.Channel.Name}**");
        }

        [Command("WelcomeStatus")]
        [Alias("ws")]
        [Summary("ws <true/false>")]
        [Remarks("sets the welcome message as true or false (on/off)")]
        public async Task WOff(bool status)
        {
            GuildConfig.SetWelcomeStatus(Context.Guild.Id, status);
            await ReplyAsync($"Welcome Messageing for this server has been set to: {status}");
        }

        [Command("SetDj")]
        [Summary("SetDj <@role>")]
        [Remarks("Sets the DJ role")]
        public async Task Dj([Remainder] IRole role)
        {
            GuildConfig.SetDj(Context.Guild.Id, role.Id);
            await ReplyAsync($"The DJ Role has been set to: {role.Name}");
        }

        [Command("Errors")]
        [Summary("Errors <true/false>")]
        [Remarks("Toggles Error Status")]
        public async Task Errors(bool status)
        {
            GuildConfig.SetError(Context.Guild.Id, status);
            if (status)
                await ReplyAsync("Errors will now be Logged");
            else
                await ReplyAsync("Errors will no longer be logged");
        }

        [Command("addrole")]
        [Summary("addrole @role")]
        [Remarks("adds a subscribable role")]
        public async Task Arole(IRole role)
        {
            GuildConfig.Subrole(Context.Guild.Id, role.Id, true);
            await ReplyAsync($"{role.Name} is now subscribable by typing `{Load.Pre}joinrole {role.Mention}`");
        }

        [Command("delrole")]
        [Summary("delrole @role")]
        [Remarks("removes the subscribable role")]
        public async Task Drole(IRole role)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));

                if (jsonObj.Roles == null || jsonObj.Roles == 0)
                {
                    await ReplyAsync("There is no subscribable role set up.");
                }
                else if (jsonObj.Roles != role.Id)
                {
                    await ReplyAsync($"{role.Name} is not the subscribable role, it cannot be removed");
                }
                else if (jsonObj.Roles == role.Id)
                {
                    GuildConfig.Subrole(Context.Guild.Id, role.Id, false);
                    await ReplyAsync($"{role.Name} is no longer subscribable");
                }
            }
            else
            {
                await ReplyAsync($"The config file does not exist, please type `{Load.Pre}setup` to initialise it");
            }
        }

        [Command("rss", RunMode = RunMode.Async)]
        [Summary("rss <feed url>")]
        [Remarks("adds an rss feed")]
        public async Task Rss(string url = null)
        {
            if (url != null)
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
                if (File.Exists(file))
                {
                    GuildConfig.RssSet(Context.Guild.Id, Context.Channel.Id, url, true);
                    await ReplyAsync("Rss Config has been updated!\n" +
                                     $"Updates will be posted in: {Context.Channel.Name}\n" +
                                     $"Url: {url}");
                }
                else
                {
                    await ReplyAsync($"The config file does not exist, please type `{Load.Pre}setup` to initialise it");
                }

                const int minutes = 5;

                while (true)
                {
                    SyndicationFeed feed;
                    try
                    {
                        var reader = XmlReader.Create(url);
                        feed = SyndicationFeed.Load(reader);
                        reader.Close();
                    }
                    catch
                    {
                        await ReplyAsync($"Error with Rss URL! {url}");
                        return;
                    }

                    foreach (var item in feed.Items)
                    {
                        var now = DateTime.UtcNow;
                        if (item.PublishDate > now.AddMinutes(-minutes) && item.PublishDate <= now)
                        {
                            var subject = item.Title.Text;
                            var link = item.Links[0].Uri.ToString();

                            await ReplyAsync($"New Post: **{subject}**\n" +
                                             $"Link: {link}");
                        }
                    }
                    await Task.Delay(1000 * 60 * minutes);
                }
            }
            GuildConfig.RssSet(Context.Guild.Id, Context.Channel.Id, null, false);
            await ReplyAsync("Rss Config has been updated! Updates will no longer be posted");
        }
    }
}