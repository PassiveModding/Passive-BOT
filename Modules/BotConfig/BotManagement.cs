using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Handlers;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
    public class BotManagement : Base
    {
        [Group("PartnerManage")]
        public class PartnerManagement : Base
        {
            [Command("Message")]
            [Summary("Message <Guild ID> <Message>")]
            [Remarks("Updates the partner message of the given server")]
            public async Task UpdateMsg(ulong ID, [Remainder] string message = null)
            {
                var guildobj = DatabaseHandler.GetGuild(ID);
                var original = guildobj.Partner.Message.Content;
                guildobj.Partner.Message.Content = message;
                await ReplyAsync(new EmbedBuilder
                {
                    Title = $"[{ID}]",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Original",
                            Value = $"{original}"
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "New",
                            Value = $"{message}"
                        }
                    }
                });
                guildobj.Save();
            }

            [Command("Ban")]
            [Summary("Ban <Guild ID>")]
            [Remarks("Toggle the ban status of a partner server")]
            public async Task BanPartner(ulong ID)
            {
                var guildobj = DatabaseHandler.GetGuild(ID);
                guildobj.Partner.Settings.Banned = !guildobj.Partner.Settings.Banned;
                await SimpleEmbedAsync($"Guild With ID: {ID}\n" +
                                       $"Banned: {guildobj.Partner.Settings.Banned}");
                guildobj.Save();
            }

            [Command("Enable")]
            [Summary("Enable <Guild ID>")]
            [Remarks("Toggle the enabled status of a partner server")]
            public async Task EnablePartner(ulong ID)
            {
                var guildobj = DatabaseHandler.GetGuild(ID);
                guildobj.Partner.Settings.Enabled = !guildobj.Partner.Settings.Enabled;
                await SimpleEmbedAsync($"Guild With ID: {ID}\n" +
                                       $"Enabled: {guildobj.Partner.Settings.Enabled}");
                guildobj.Save();
            }

            [Command("PartnerList")]
            [Summary("PartnerList <Choice>")]
            [Remarks("Get a complete list of all partner servers")]
            public async Task PListF2(int choice = 0)
            {
                if (choice <= 0 || choice >= 4)
                {
                    await SimpleEmbedAsync("Please choose a Sorting Mode:\n" +
                                     "`1` - Full List Includes all other checks\n" +
                                     "`2` - Short List, Removes all banned partners\n" +
                                     "`3` - Visibility, Shows where the visible users is not equal to the total users");
                    return;
                }

                var gobjs = DatabaseHandler.GetFullConfig().Where(x => x.Partner.Settings.Enabled).ToList();
                var pages = new List<PaginatedMessage.Page>();
                var search = gobjs;
                if (choice == 2)
                {
                    search = gobjs.Where(x => x.Partner.Settings.Banned == false).ToList();
                }
                else if (choice == 3)
                {
                    search = gobjs.Where(x => Context.Socket.Client.GetChannel(x.Partner.Settings.ChannelID)?.Users.Count != Context.Socket.Client.GetGuild(x.ID)?.Users.Count).ToList();
                }

                foreach (var guildModel in search)
                {
                    if (Context.Socket.Client.GetGuild(guildModel.ID) is SocketGuild pGuild)
                    {
                        var fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Guild Info",
                                Value = $"ID: {pGuild.Id}\n" +
                                        $"Owner: {pGuild.Owner}\n" +
                                        $"Users: {pGuild.MemberCount}"
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Partner Settings",
                                Value = $"Enabled: {guildModel.Partner.Settings.Enabled}\n" +
                                        $"Banned: {guildModel.Partner.Settings.Banned}\n" +
                                        $"Channel ID: {guildModel.Partner.Settings.ChannelID}\n" +
                                        $"Show UserCount: {guildModel.Partner.Message.UserCount}\n" +
                                        $"Image Url: {guildModel.Partner.Message.ImageUrl ?? "N/A"}\n" +
                                        $"Thumbnail: {guildModel.Partner.Message.UseThumb}"
                            }
                        };
                        if (pGuild.GetChannel(guildModel.Partner.Settings.ChannelID) is SocketTextChannel pChannel)
                        {
                            var ChannelOverWrites = pChannel.PermissionOverwrites;
                            var Checking = new StringBuilder();
                            foreach (var OverWrite in ChannelOverWrites)
                                try
                                {
                                    var Name = "N/A";
                                    if (OverWrite.TargetType == PermissionTarget.Role)
                                    {
                                        var Role = pGuild.Roles.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                                        if (Role != null) Name = Role.Name;
                                    }
                                    else
                                    {
                                        var user = pGuild.Users.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                                        if (user != null) Name = user.Username;
                                    }

                                    if (OverWrite.Permissions.ViewChannel == PermValue.Deny)
                                        Checking.AppendLine($"{Name} Cannot Read Msgs.");

                                    if (OverWrite.Permissions.ReadMessageHistory == PermValue.Deny)
                                        Checking.AppendLine($"{Name} Cannot Read History.");
                                }
                                catch
                                {
                                    //
                                }

                            var userstring = $"Users Visible: [{pChannel.Users.Count} / {pGuild.Users.Count}] [{(double) pChannel.Users.Count / pGuild.Users.Count * 100}%]";
                            fields.Add(new EmbedFieldBuilder
                            {
                                Name = "Visibility Settings",
                                Value = $"{userstring}\n" +
                                        $"Channel: {pChannel.Name}\n" +
                                        "Permissions:\n" +
                                        $"{Checking}"
                            });
                        }

                        fields.Add(new EmbedFieldBuilder
                        {
                            Name = "Message",
                            Value = $"{guildModel.Partner.Message.Content ?? "N/A"}"
                        });

                        pages.Add(new PaginatedMessage.Page
                        {
                            dynamictitle = pGuild.Name,
                            Fields = fields,
                            imageurl = guildModel.Partner.Message.ImageUrl
                        });
                    }
                    else
                    {
                        guildModel.Partner.Settings.Enabled = false;
                    }
                }

                if (pages.Any())
                {
                    var msg = new PaginatedMessage
                    {
                        Title = "Partner Messages",
                        Pages = pages,
                        Color = Color.Green
                    };

                    await PagedReplyAsync(msg);
                }
                else
                {
                    await SimpleEmbedAsync("No Servers detected.");
                }
            }
        }

        /*
        [Command("Test2")]
        [Summary("Test2")]
        [Remarks("x")]
        public async Task T2()
        {
            try
            {
                var graph = new PlotModel
                {
                    PlotType = PlotType.XY
                };
                var timegroup = CommandHandler.GuildMsgs
                    .First(x => x.GuildID == Context.Guild.Id)
                    .Times
                    .Where(x => x > DateTime.UtcNow - TimeSpan.FromHours(1))
                    .GroupBy(x => x.Second)
                    .OrderBy(x => x.First().Ticks)
                    .ToList();
                var series = new LineSeries();
                for (int i = 0; i < 60; i++)
                {
                    var part = timegroup.FirstOrDefault(x => x.First().Second == i);
                    if (part == null)
                    {
                        series.Points.Add(new DataPoint(i, 0));
                    }
                    else
                    {
                        series.Points.Add(new DataPoint(i, part.Count()));
                    }
                }

                graph.Series.Add(series);
                //graph.DrawLines(pen, points.ToArray());
                using (var ms = new MemoryStream())
                {
                    //image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    //Stream MStream = new MemoryStream();
                    PdfExporter.Export(graph, ms, 1000, 500);
                    MemoryStream ms2 = new MemoryStream(ms.ToArray());
                    ms2.Seek(0, SeekOrigin.Begin);
                    await Context.Channel.SendFileAsync(ms2, "Test.pdf");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public int Flip(int input, int total = 1000)
        {
            return total - input;
        }*/
    }
}