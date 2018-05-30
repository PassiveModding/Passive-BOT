using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.Info
{
    [RequireContext(ContextType.Guild)]
    [Group("Tag")]
    [Alias("Tags")]
    public class Tags : Base
    {
        [Command("add")]
        [Summary("add <name> <message>")]
        [Remarks("adds a tag to the server")]
        public async Task Tagadd(string tagname, [Remainder] string tagmessage)
        {
            if (Context.Server.Tags.Settings.Enabled)
            {
                if (Context.Server.Tags.Settings.AdminOnly)
                {
                    if (!CheckAdmin.IsAdmin(Context))
                    {
                        throw new Exception("Only Admins can Create tags.");
                    }
                }

                if (Context.Server.Tags.Tags.Any(x => string.Equals(x.Name, tagname, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception("There is already a tag with this name in the server. Please delete it then add the new tag.");
                }

                var tg = new GuildModel.tags.tag
                {
                    Name = tagname,
                    Content = tagmessage,
                    CreatorID = Context.User.Id,
                    OwnerName = $"{Context.User}"
                };
                Context.Server.Tags.Tags.Add(tg);
                Context.Server.Save();
                await SimpleEmbedAsync("Tag Added!");
            }
            else
            {
                throw new Exception("Tagging is not enabled in this server!");
            }
        }

        [Command("del")]
        [Summary("del <name>")]
        [Remarks("Removes a tag from the server")]
        public async Task Tagdel(string tagname)
        {
            if (Context.Server.Tags.Tags.Count > 0)
            {
                var tag = Context.Server.Tags.Tags.FirstOrDefault(x => string.Equals(x.Name, tagname, StringComparison.CurrentCultureIgnoreCase));

                if (tag == null)
                {
                    throw new Exception("Invalid Tag Name");
                }

                if (CheckAdmin.IsAdmin(Context))
                {
                    Context.Server.Tags.Tags.Remove(tag);
                    await SimpleEmbedAsync("Tag Deleted using Admin Permissions");
                }
                else if (tag.CreatorID == Context.User.Id)
                {
                    Context.Server.Tags.Tags.Remove(tag);
                    await SimpleEmbedAsync("Tag Deleted By Owner");
                }
                else
                {
                    await SimpleEmbedAsync("You do not own this tag");
                }

                Context.Server.Save();
            }
            else
            {
                throw new Exception("This server has no tags.");
            }
        }

        [Command]
        [Summary("[optional]<tagname>")]
        [Remarks("lists all tags for the server or sends a tag")]
        public async Task Tag(string tagname = null)
        {
            if (tagname == null)
            {
                var tags = Context.Server.Tags.Tags;
                if (tags.Count > 0)
                {
                    var taglist = string.Join(", ", tags.Select(x => x.Name));
                    await ReplyAsync($"**Tags:**\n{taglist}");
                }
                else
                {
                    await ReplyAsync("This server has no tags yet.");
                }
            }
            else
            {
                var embed = new EmbedBuilder();
                if (Context.Server.Tags.Tags.Count > 0)
                {
                    var tag = Context.Server.Tags.Tags.FirstOrDefault(x => string.Equals(x.Name, tagname, StringComparison.CurrentCultureIgnoreCase));
                    if (tag == null)
                    {
                        await ReplyAsync($"No tag with the name **{tagname}** exists.");
                    }
                    else
                    {
                        var own = await Context.Guild.GetUserAsync(tag.CreatorID);
                        var ownername = own?.Username ?? tag.OwnerName;

                        embed.AddField(tag.Name, tag.Content);
                        embed.WithFooter(x =>
                        {
                            x.Text =
                                $"Tag Owner: {ownername} || Uses: {tag.Uses} || Command Invokee: {Context.User.Username}";
                        });
                        tag.Uses++;
                        Context.Server.Save();
                        await SendEmbedAsync(embed.Build());
                    }
                }
            }
        }
    }
}