namespace PassiveBOT.Modules.GlobalCommands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;
    using Discord.WebSocket;

    using PassiveBOT.Context;

    public class Inspect : Base
    {
        [Command("inspect")]
        public Task InspectRole(SocketRole role)
        {
            var pages = new List<PaginatedMessage.Page>();

            string userString;
            if (role.Members.Count() <= 10)
            {
                userString = string.Join("\n", role.Members.Select(x => x.Mention));
            }
            else
            {
                userString = $"Count: {role.Members.Count()}";
            }

            var p = role.Permissions;

            pages.Add(new PaginatedMessage.Page { Fields = new List<EmbedFieldBuilder> { new EmbedFieldBuilder { Name = "User Info", Value = $"{userString}" }, new EmbedFieldBuilder { Name = "Permissions", Value = "**Management**\n" + $"Administrator: {p.Administrator}\n" + $"ManageGuild: {p.ManageGuild}\n" + $"ManageRoles: {p.ManageRoles}\n" + $"ManageMessages: {p.ManageMessages}\n" + $"ManageChannels: {p.ManageChannels}\n" + $"ManageEmojis: {p.ManageEmojis}\n" + $"ManageNicknames: {p.ManageNicknames}\n" + $"ManageWebhooks: {p.ManageWebhooks}\n" + $"KickMembers: {p.KickMembers}\n" + $"BanMembers: {p.BanMembers}\n" + $"DeafenMembers: {p.DeafenMembers}\n" + $"MoveMembers: {p.MoveMembers}\n" + $"MuteMembers: {p.MuteMembers}\n" + "**Text**\n" + $"SendMessages: {p.SendMessages}\n" + $"ReadMessages: {p.ViewChannel}\n" + $"ReadMessageHistory: {p.ReadMessageHistory}\n" + $"AddReactions: {p.AddReactions}\n" + $"EmbedLinks: {p.EmbedLinks}\n" + $"AttachFiles: {p.AttachFiles}\n" + $"MentionEveryone: {p.MentionEveryone}\n" + $"SendTTSMessages: {p.SendTTSMessages}\n" + $"UseExternalEmojis: {p.UseExternalEmojis}\n" + "**VOICE**\n" + $"Connect: {p.Connect}\n" + $"Speak: {p.Speak}\n" + "**Self**\n" + $"ChangeNickname: {p.ChangeNickname}\n" + $"CreateInstantInvite: {p.CreateInstantInvite}\n" } } });

            return PagedReplyAsync(new PaginatedMessage { Pages = pages }, new ReactionList { Forward = true, Backward = true });
        }
    }
}