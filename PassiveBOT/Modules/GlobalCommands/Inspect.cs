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
    using PassiveBOT.Extensions;

    public class Inspect : Base
    {
        [Command("inspect")]
        public Task InspectRoleAsync(SocketRole role)
        {
            var pages = new List<PaginatedMessage.Page>();

            var p = role.Permissions;

            pages.Add(
                new PaginatedMessage.Page
                {
                    Fields = new List<EmbedFieldBuilder>
                                     {
                                         new EmbedFieldBuilder
                                             {
                                                 Name =
                                                     "Permissions",
                                                 Value =
                                                     "**Management**\n"
                                                     + $"Administrator: {p.Administrator}\n"
                                                     + $"ManageGuild: {p.ManageGuild}\n"
                                                     + $"ManageRoles: {p.ManageRoles}\n"
                                                     + $"ManageMessages: {p.ManageMessages}\n"
                                                     + $"ManageChannels: {p.ManageChannels}\n"
                                                     + $"ManageEmojis: {p.ManageEmojis}\n"
                                                     + $"ManageNicknames: {p.ManageNicknames}\n"
                                                     + $"ManageWebhooks: {p.ManageWebhooks}\n"
                                                     + $"KickMembers: {p.KickMembers}\n"
                                                     + $"BanMembers: {p.BanMembers}\n"
                                                     + $"DeafenMembers: {p.DeafenMembers}\n"
                                                     + $"MoveMembers: {p.MoveMembers}\n"
                                                     + $"MuteMembers: {p.MuteMembers}\n"
                                                     + "**Text**\n"
                                                     + $"SendMessages: {p.SendMessages}\n"
                                                     + $"ReadMessages: {p.ViewChannel}\n"
                                                     + $"ReadMessageHistory: {p.ReadMessageHistory}\n"
                                                     + $"AddReactions: {p.AddReactions}\n"
                                                     + $"EmbedLinks: {p.EmbedLinks}\n"
                                                     + $"AttachFiles: {p.AttachFiles}\n"
                                                     + $"MentionEveryone: {p.MentionEveryone}\n"
                                                     + $"SendTTSMessages: {p.SendTTSMessages}\n"
                                                     + $"UseExternalEmojis: {p.UseExternalEmojis}\n"
                                                     + "**VOICE**\n"
                                                     + $"Connect: {p.Connect}\n"
                                                     + $"Speak: {p.Speak}\n"
                                                     + "**Self**\n"
                                                     + $"ChangeNickname: {p.ChangeNickname}\n"
                                                     + $"CreateInstantInvite: {p.CreateInstantInvite}\n"
                                             }
                                     }
                });

            int i = 0;
            foreach (var userGroup in role.Members.ToList().SplitList(20))
            {
                i++;
                pages.Add(new PaginatedMessage.Page
                {
                    Fields = new List<EmbedFieldBuilder>
                                               {
                                                   new EmbedFieldBuilder
                                                       {
                                                           Name = $"Users ({i})",
                                                           Value = string.Join("\n", userGroup.Select(u => u.Mention))
                                                       }
                                               }
                });
            }

            return PagedReplyAsync(new PaginatedMessage { Pages = pages }, new ReactionList { Forward = true, Backward = true });
        }

        [Command("inspect")]
        public Task InspectUserAsync(SocketGuildUser user)
        {
            var pages = new List<PaginatedMessage.Page>();

            var p = user.GuildPermissions;

            pages.Add(new PaginatedMessage.Page
            {
                Fields = new List<EmbedFieldBuilder>
                                     {
                                         new EmbedFieldBuilder
                                             {
                                                 Name =
                                                     "User Info",
                                                 Value =
                                                     $"Mention: {user.Mention}\n" +
                                                     $"UserName: {user.Username}\n" +
                                                     $"NickName: {user.Nickname ?? "N/A"}\n" +
                                                     $"Discriminator: #{user.Discriminator}\n" +
                                                     $"ID: {user.Id}\n" +
                                                     $"Status: {user.Status}\n" +
                                                     $"Activity: {user.Activity?.Name} {user.Activity?.Type}\n" +
                                                     $"Avatar Url: {user.GetAvatarUrl()}\n" +
                                                     $"Default Avatar Url: {user.GetDefaultAvatarUrl()}\n" +
                                                     $"Created At: {user.CreatedAt}\n"
                                             }
                                     }
            });

            pages.Add(new PaginatedMessage.Page
            {
                Fields = new List<EmbedFieldBuilder>
                          {
                                         new EmbedFieldBuilder
                                             {
                                                 Name = "Guild Info",
                                                 Value = $"Hierarchy: {user.Hierarchy}\n" +
                                                         $"Roles:\n" +
                                                         $"{string.Join("\n", user.Roles.OrderByDescending(x => x.Position).Select(r => r.Mention))}\n" +
                                                         $"Is Bot: {user.IsBot}\n" +
                                                         $"Joined at: {user.JoinedAt?.DateTime}\n" +
                                                         $"**Voice**\n" +
                                                         $"Deafened: {user.IsDeafened}\n" +
                                                         $"Muted: {user.IsMuted}\n" +
                                                         $"Self Deafened: {user.IsSelfDeafened}\n" +
                                                         $"Self Muted: {user.IsSelfMuted}\n" +
                                                         $"Suppressed: {user.IsSuppressed}\n" +
                                                         $"Voice Channel: {user.VoiceChannel?.Name ?? "N/A"}\n" +
                                                         $"Voice Session ID: {user.VoiceSessionId}\n"
                                             }
                          }
            });

            pages.Add(new PaginatedMessage.Page
            {
                Fields = new List<EmbedFieldBuilder>
                          {
                                         new EmbedFieldBuilder
                                             {
                                                 Name =
                                                     "Permissions",
                                                 Value =
                                                     "**Management**\n"
                                                     + $"Administrator: {p.Administrator}\n"
                                                     + $"ManageGuild: {p.ManageGuild}\n"
                                                     + $"ManageRoles: {p.ManageRoles}\n"
                                                     + $"ManageMessages: {p.ManageMessages}\n"
                                                     + $"ManageChannels: {p.ManageChannels}\n"
                                                     + $"ManageEmojis: {p.ManageEmojis}\n"
                                                     + $"ManageNicknames: {p.ManageNicknames}\n"
                                                     + $"ManageWebhooks: {p.ManageWebhooks}\n"
                                                     + $"KickMembers: {p.KickMembers}\n"
                                                     + $"BanMembers: {p.BanMembers}\n"
                                                     + $"DeafenMembers: {p.DeafenMembers}\n"
                                                     + $"MoveMembers: {p.MoveMembers}\n"
                                                     + $"MuteMembers: {p.MuteMembers}\n"
                                                     + "**Text**\n"
                                                     + $"SendMessages: {p.SendMessages}\n"
                                                     + $"ReadMessages: {p.ViewChannel}\n"
                                                     + $"ReadMessageHistory: {p.ReadMessageHistory}\n"
                                                     + $"AddReactions: {p.AddReactions}\n"
                                                     + $"EmbedLinks: {p.EmbedLinks}\n"
                                                     + $"AttachFiles: {p.AttachFiles}\n"
                                                     + $"MentionEveryone: {p.MentionEveryone}\n"
                                                     + $"SendTTSMessages: {p.SendTTSMessages}\n"
                                                     + $"UseExternalEmojis: {p.UseExternalEmojis}\n"
                                                     + "**VOICE**\n"
                                                     + $"Connect: {p.Connect}\n"
                                                     + $"Speak: {p.Speak}\n"
                                                     + "**Self**\n"
                                                     + $"ChangeNickname: {p.ChangeNickname}\n"
                                                     + $"CreateInstantInvite: {p.CreateInstantInvite}\n"
                                             }
                          }
            });

            return PagedReplyAsync(new PaginatedMessage { Pages = pages }, new ReactionList { Forward = true, Backward = true });
        }

        // TODO Channel, Current guild
    }
}