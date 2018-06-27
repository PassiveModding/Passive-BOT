namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions.PassiveBOT;
    using PassiveBOT.Models;

    /// <summary>
    /// The tags module.
    /// </summary>
    [RequireContext(ContextType.Guild)]
    [Group("Tag")]
    [Alias("Tags")]
    [Summary("Tags are like shortcuts for messages, you can use one to have the bot respond with a specific message that you have pre-set.")]
    public class Tags : Base
    {
        /// <summary>
        /// adds a tag
        /// </summary>
        /// <param name="tagName">
        /// The tagName.
        /// </param>
        /// <param name="tagMessage">
        /// The tagMessage.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if a tag exists with the name or if admin check fails
        /// </exception>
        [Command("add")]
        [Summary("adds a tag to the server")]
        public async Task AddTagAsync(string tagName, [Remainder] string tagMessage)
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

                if (Context.Server.Tags.Tags.Any(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception("There is already a tag with this name in the server. Please delete it then add the new tag.");
                }

                var tg = new GuildModel.TagSetup.Tag
                {
                    Name = tagName,
                    Content = tagMessage,
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

        /// <summary>
        /// Deletes a Tag
        /// </summary>
        /// <param name="tagName">
        /// The tagName.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception"> throws if the name is invalid
        /// </exception>
        [Command("del")]
        [Summary("Removes a tag from the server")]
        public async Task DelTagAsync(string tagName)
        {
            if (Context.Server.Tags.Tags.Count > 0)
            {
                var tag = Context.Server.Tags.Tags.FirstOrDefault(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase));

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

        /// <summary>
        /// gets a tag or tag list
        /// </summary>
        /// <param name="tagName">
        /// The tag name.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command]
        [Summary("Gets a tag")]
        [Remarks("Lists all tag names if none provided")]
        public async Task TagAsync(string tagName = null)
        {
            if (tagName == null)
            {
                var tags = Context.Server.Tags.Tags;
                if (tags.Count > 0)
                {
                    var tagList = string.Join(", ", tags.Select(x => x.Name));
                    await ReplyAsync($"**Tags:**\n{tagList}");
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
                    var tag = Context.Server.Tags.Tags.FirstOrDefault(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase));
                    if (tag == null)
                    {
                        await ReplyAsync($"No tag with the name **{tagName}** exists.");
                    }
                    else
                    {
                        var own = Context.Guild.GetUser(tag.CreatorID);
                        var ownerName = own?.Username ?? tag.OwnerName;

                        embed.AddField(tag.Name, tag.Content);
                        embed.WithFooter(x =>
                        {
                            x.Text =
                                $"Tag Owner: {ownerName} || Uses: {tag.Uses} || Command Invoker: {Context.User.Username}";
                        });
                        tag.Uses++;
                        Context.Server.Save();
                        await ReplyAsync(embed.Build());
                    }
                }
            }
        }
    }
}