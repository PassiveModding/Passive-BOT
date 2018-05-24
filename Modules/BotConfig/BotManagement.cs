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
using Color = Discord.Color;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
    public class BotManagement : Base
    {
        [Command("PartnerList+", RunMode = RunMode.Async)]
        [Summary("PartnerList+")]
        [Remarks("Get a complete list of all partner servers")]
        public async Task PListF2(int choice = 0)
        {
            if (choice <= 0 || choice >= 4)
            {
                await ReplyAsync("Please choose a Sorting Mode:\n" +
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
                        var userstring = $"Users Visible: [{pChannel.Users.Count} / {pGuild.Users.Count}] [{(double)pChannel.Users.Count / pGuild.Users.Count * 100}%]";
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
                await ReplyAsync("No Servers detected.");
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
                System.Drawing.Image image = new Bitmap(1000, 500);
                var graph = Graphics.FromImage(image);
                graph.Clear(System.Drawing.Color.Azure);
                var pen = new Pen(Brushes.Black);

                var points = new List<Point>
                {
                    new Point(0, 1000),
                    //new Point(100, 500),
                    //new Point(150, 100)
                };

            
                foreach (var time in CommandHandler.GuildMsgs.First(x => x.guildID == Context.Guild.Id).times)
                {
                    var x = (time.Ticks - (DateTime.UtcNow-TimeSpan.FromHours(1)).Ticks)/100000000;
                    var y = (time.Ticks - (DateTime.UtcNow - TimeSpan.FromHours(1)).Ticks) / 100000000;
                    Console.WriteLine(x);
                    Console.WriteLine(y);
                    points.Add(new Point((int)x, (int)y));
                }
            
                graph.DrawLines(pen, points.ToArray());
                Stream memoryStream = new MemoryStream();
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(memoryStream, "Test.PNG");
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
