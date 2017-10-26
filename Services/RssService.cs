using System;
using System.Collections.Concurrent;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using Color = System.Drawing.Color;

namespace PassiveBOT.Services
{
    public class RssService
    {
        public ConcurrentDictionary<ulong, Timer> Guild = new ConcurrentDictionary<ulong, Timer>();

        public async Task Rss(string urlin, IGuildChannel channel)
        {
            if (urlin == null)
            {
                Guild.TryRemove(channel.Id, out Timer _);
                GuildConfig.RssSet(channel.Guild, channel.Id, null, false);
                await ((ITextChannel) channel).SendMessageAsync(
                    "Rss Config has been updated! Updates will no longer be posted");
            }
            else
            {
                try
                {
                    const int minutes = 5;
                    var t = new Timer(async _ =>
                    {
                        try
                        {
                            SyndicationFeed feed;

                            var url = GuildConfig.Load(channel.GuildId).Rss;

                            if (url == "0" || url == null)
                                return;
                            try
                            {
                                var reader = XmlReader.Create(url);
                                feed = SyndicationFeed.Load(reader);
                                reader.Close();
                            }
                            catch
                            {
                                await ((ITextChannel) channel).SendMessageAsync($"Error loading Rss URL! {url}\n" +
                                                                                "Ending Feed");
                                Guild.TryRemove(channel.Id, out Timer _);
                                return;
                            }

                            var i = 0;
                            foreach (var item in feed.Items)
                            {
                                var now = DateTime.UtcNow;
                                if (i == 4)
                                {
                                    await ((ITextChannel) channel).SendMessageAsync(
                                        $"The Maximum PPC(post per cycle) has been hit, Limiting updates for {minutes} min(s).");
                                    return;
                                }
                                if (item.PublishDate >= now.AddMinutes(-minutes))
                                {
                                    var subject = item.Title.Text;
                                    var link = item.Links[0].Uri.ToString();

                                    var embed = new EmbedBuilder();
                                    embed.AddField("RSS Update", $"Post: [{subject}]({link})\n" +
                                                                 $"Feed: [RSS Link]({url})");
                                    embed.WithFooter(x => { x.WithText($"{item.PublishDate}"); });

                                    try
                                    {
                                        await ((ITextChannel) channel).SendMessageAsync("", false, embed.Build());
                                    }
                                    catch
                                    {
                                        await ((ITextChannel) channel).SendMessageAsync($"New Post: **{subject}**\n" +
                                                                                        $"Link: {link}");
                                    }
                                    await ColourLog.In3("RSS", 'R', channel.Guild.Name, 'L', link, Color.Teal);
                                    i++;
                                    await Task.Delay(1000);
                                }
                            }
                        }
                        catch
                        {
                            //
                        }
                    }, null, 0, minutes * 1000 * 60);
                    Guild.AddOrUpdate(channel.Id, t, (key, old) =>
                    {
                        old.Change(Timeout.Infinite, Timeout.Infinite);
                        return t;
                    });

                    await ColourLog.In2("RSS", 'R', channel.Guild.Name, Color.Teal);
                }
                catch
                {
                    await ColourLog.In2Error("RSS Error", 'R', channel.Guild.Name);
                }
            }
        }
    }
}