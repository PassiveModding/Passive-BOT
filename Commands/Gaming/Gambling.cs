using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands.Gaming
{
    public class Gambling : InteractiveBase
    {
        [Command("DailyReward", RunMode = RunMode.Async)]
        [Summary("DailyReward")]
        [Remarks("Get 200 free coins for PassiveBOT Gambling")]
        [Ratelimit(1, 1, Measure.Days)]
        public async Task DailyReward()
        {
            await Setupuser(Context.Guild, Context.User);
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var guser = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            guser.coins = guser.coins + 200;
            GuildConfig.SaveServer(guildobj);
            var embed = new EmbedBuilder
            {
                Title = "Success, you have received 200 coins",
                Description = $"Balance: {guser.coins}",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("GambleStats", RunMode = RunMode.Async)]
        [Summary("GambleStats")]
        [Remarks("Get a user's gambling stats")]
        public async Task DailyReward(IUser user)
        {
            await Setupuser(Context.Guild, Context.User);
            var guildobj = GuildConfig.GetServer(Context.Guild);
            if (user == null)
            {
                user = Context.User;
            }
            var guser = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == user.Id);

            var embed = new EmbedBuilder
            {
                Title = $"{user.Username} Gambling Stats",
                Description = $"Balance: {guser.coins}\n" +
                              $"Total Bet: {guser.totalbet}\n" +
                              $"Total Paid Out: {guser.totalpaidout}\n",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Slots", RunMode = RunMode.Async)]
        [Summary("Slots <bet>")]
        [Remarks("Play Slots")]
        public async Task Slots(int bet = 0)
        {
            await Setupuser(Context.Guild, Context.User);

            //Initially we check wether or not the user is able to bet
            if (bet <= 0)
            {
                await ReplyAsync("Please place a bet, ie. 10 coins!");
                return;
            }
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var guser = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            if (bet > guser.coins)
            {
                await ReplyAsync($"Your bet is too high, please place a bet less than or equal to {guser.coins}");
                return;
            }
            //now we deduct the bet amount from the users balance
            guser.coins = guser.coins - bet;



            var itemlist = new List<string>
            {
                "💯", //:100:
                "🌻", //:sunflower:
                "🌑", //:new_moon:
                "🐠", //:tropical_fish: 
                "🎄", //:christmas_tree: 
                "👾", // space invaders
                "⚽" // soccer ball

            };

            var selections = new string[3];
            for (var i = 0; i < selections.Length; i++)
            {
                selections[i] = itemlist[new Random().Next(0, itemlist.Count)];
            }

            //Winning Combos
            //Three of any
            //3, 2 or 1, :100:'s
            //3 XMAS Trees
            var multiplier = 0;
            if (selections.All(x => x == "🎄"))
            {
                multiplier = 30;
            }
            else if (selections.All(x => x == selections[0]))
            {
                multiplier = 10;
            }
            else if (selections.Count(x => x == "💯") > 0)
            {
                multiplier = selections.Count(x => x == "💯");
            }

            var payout = bet * multiplier;

            guser.coins = guser.coins + payout;
            guser.totalpaidout = guser.totalpaidout + payout;
            guser.totalbet = guser.totalbet + bet;
            GuildConfig.SaveServer(guildobj);
            var embed = new EmbedBuilder
            {
                Title = "SLOTS",
                Description = $"➡️ {selections[0]}{selections[1]}{selections[2]} ⬅️\n\n" +
                              $"BET: {bet}\n" +
                              $"PAY: {payout}\n" +
                              $"BAL: {guser.coins}",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("DoubleOrNothing", RunMode = RunMode.Async)]
        [Summary("DoubleOrNothing <bet>")]
        [Remarks("50/50 chance to double your money")]
        public async Task Double(int bet = 0)
        {
            await Setupuser(Context.Guild, Context.User);
            //Initially we check wether or not the user is able to bet
            if (bet <= 0)
            {
                await ReplyAsync("Please place a bet, ie. 10 coins!");
                return;
            }
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var guser = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            if (bet > guser.coins)
            {
                await ReplyAsync($"Your bet is too high, please place a bet less than or equal to {guser.coins}");
                return;
            }
            //now we deduct the bet amount from the users balance
            guser.coins = guser.coins - bet;
            var win = new Random().Next(0, 100) < 50;
            var payout = win ? bet * 2 : 0;
            guser.coins = guser.coins + payout;
            guser.totalbet = guser.totalbet + bet;
            guser.totalpaidout = guser.totalpaidout + payout;
            if (win)
            {
                var embed = new EmbedBuilder
                {
                    Title = "You Successfully doubled your money",
                    Description = $"BET: {bet}\n" +
                                  $"PAY: {payout}\n" +
                                  $"BAL: {guser.coins}",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = Color.Blue,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                    }
                };
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Title = "Sorry You Lost",
                    Description = $"BET: {bet}\n" +
                                  $"PAY: {payout}\n" +
                                  $"BAL: {guser.coins}",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = Color.Blue,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                    }
                };
                await ReplyAsync("", false, embed.Build());
            }
            GuildConfig.SaveServer(guildobj);

        }

        [Command("Connect4", RunMode = RunMode.Async)]
        [Summary("Connect4 <bet>")]
        [Remarks("Play connect 4 with another person")]
        public async Task Connect4(int bet = 0)
        {
            try
            {
                var currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
                if (currentlobby != null)
                {
                    if (currentlobby.gamerunning)
                    {
                        await ReplyAsync("A game of connect4 is already running in this channel. Please wait until it is completed.");
                        return;
                    }
                    else
                    {
                        currentlobby.gamerunning = true;
                        Console.WriteLine("Game Running Set = True");
                    }

                }
                else
                {
                    Console.WriteLine("Lobby Added");
                    CommandHandler.Connect4List.Add(new CommandHandler.Con4GameList
                    {
                        channelID = Context.Channel.Id,
                        gamerunning = true
                    });
                    currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
                }
                await Setupuser(Context.Guild, Context.User);
                //Initially we check wether or not the user is able to bet
                if (bet <= 0)
                {
                    await ReplyAsync("Please place a bet, ie. 10 coins!");
                    currentlobby.gamerunning = false;
                    return;
                }
                var initguildobj = GuildConfig.GetServer(Context.Guild);
                var initplayer1 = initguildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
                if (bet > initplayer1.coins)
                {
                    await ReplyAsync($"Your bet is too high, please place a bet less than or equal to {initplayer1.coins}");
                    currentlobby.gamerunning = false;
                    return;
                }
                //now we deduct the bet amount from the users balance
                GuildConfig.gambling.user p2 = null;
                bool accepted = false;

                await ReplyAsync("", false, new EmbedBuilder
                { Description = "Get somebody to type `connect4 accept` to start" });

                var timeoutattempts = 0;
                var messagewashout = 0;
                while (!accepted)
                {
                    //wait for another player to accept
                    var next = await NextMessageAsync(false, true, TimeSpan.FromSeconds(10));
                    if (next?.Author.Id == Context.User.Id)
                    {
                        //Ignore author messages for the game until another player accepts
                    }
                    else if (next?.Content.ToLower() == "connect4 accept".ToLower())
                    {
                        Console.WriteLine("Accept msg detected");
                        await Setupuser(Context.Guild, next.Author);
                        var guildobj2 = GuildConfig.GetServer(Context.Guild);
                        p2 = guildobj2.Gambling.Users.FirstOrDefault(x => x.userID == next.Author.Id);
                        if (p2.coins < bet)
                        {
                            Console.WriteLine("Player2 insufficient balance Connect4");
                            await ReplyAsync(
                                $"{next.Author.Mention} - You do not have enough coins to play this game, you need a minimum of {bet}. Your balance is {p2.coins}");
                        }
                        else
                        {
                            Console.WriteLine("player2 accepted Connect4");
                            accepted = true;
                            p2 = guildobj2.Gambling.Users.FirstOrDefault(x => x.userID == next.Author.Id);
                        }
                    }

                    if (next == null)
                    {
                        timeoutattempts++;
                        if (timeoutattempts >= 6)
                        {
                            await ReplyAsync("Connect4: Timed out!");
                            currentlobby.gamerunning = false;
                            return;
                        }
                    }
                    else
                    {
                        messagewashout++;
                        if (messagewashout > 25)
                        {
                            await ReplyAsync("Connect4: Timed out!");
                            currentlobby.gamerunning = false;
                            return;
                        }
                    }
                }

                var initgameguildobj = GuildConfig.GetServer(Context.Guild);
                var player1 = initgameguildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
                var player2 = initgameguildobj.Gambling.Users.FirstOrDefault(x => x.userID == p2.userID);

                var lines = new int[6, 7];
                var embed = new EmbedBuilder();
                string none = ":black_circle:";
                string blue = ":large_blue_circle:";
                string red = ":red_circle:";
                var numlist = new List<string>
                {
                    "zero",
                    "one",
                    "two",
                    "three",
                    "four",
                    "five",
                    "six",
                    "seven",
                    "eight",
                    "nine"
                };

                for (int r = 0; r < 6; r++)
                {
                    if (r == 0)
                    {
                        for (int c = 0; c < 7; c++)
                        {
                            embed.Description += $":{numlist[c]}:";
                        }
                        embed.Description += "\n";
                    }
                    for (int c = 0; c < 7; c++)
                    {
                        if (lines[r, c] == 0)
                        {
                            embed.Description += $"{none}";
                        }
                        else if (lines[r, c] == 1)
                        {
                            embed.Description += $"{blue}";
                        }
                        else if (lines[r, c] == 2)
                        {
                            embed.Description += $"{red}";
                        }
                    }

                    embed.Description += "\n";
                }

                embed.Description += "Usage:\n" +
                                     "`connect4 [column]`\n" +
                                     $":large_blue_circle: - {Context.User.Mention}\n" +
                                     $":red_circle: - {Context.Guild.GetUser(player2.userID)?.Mention}";
                var game = await ReplyAsync("", false, embed.Build());
                //0 = draw
                //1 = player1
                //2 = player2
                
                var lastx = 0;
                var lasty = 0;
                var playinggame = true;
                var currentplayer = 1;
                var winmethod = "";
                var msgtime = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                while (playinggame)
                {
                    var next = await NextMessageAsync(false, true, TimeSpan.FromMinutes(1));
                    //filter out non game messages by ignoring ones
                    if (next == null || msgtime < DateTime.UtcNow)
                    {
                        //If the player doesn't show up mark them as forfeitting
                        await ReplyAsync(
                            $"{(currentplayer == 1 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)} Did not reply fast enough. Auto forfitting");

                        var w = currentplayer == 1 ? player2.userID : player1.userID;
                        var l = currentplayer == 1 ? player1.userID : player2.userID;
                        await Connect4Win(w,l, bet, "Player Forfitted.");
                        return;
                    }
                    if (!next.Content.ToLower().StartsWith("connect4")) continue;
                    if (next.Author.Id != player1.userID && next.Author.Id != player2.userID)
                    {
                        await ReplyAsync("You are not part of this game.");
                    }
                    else if ((next.Author.Id == player1.userID && currentplayer == 1) || (next.Author.Id == player2.userID && currentplayer == 2))
                    {
                        //filter out invalid line submissions
                        var parameters = next.Content.Split(" ");
                        if (parameters.Length > 1)
                        {
                            var param = next.Content.Split(" ")[1];
                            if (int.TryParse(param, out int Column))
                            {
                                if (Column < 0 || Column > 6)
                                {
                                    //error invalid line.
                                    await ReplyAsync("Invalid input, line number must be from 0-6 message in the format:\n" +
                                                     "`connect4 [line]`");
                                }
                                else
                                {
                                    bool success = false;
                                    //moving from the top of the board downwards
                                    for (int Row = 5; Row >= 0; Row--)
                                    {
                                        if (lines[Row, Column] == 0)
                                        {
                                            lines[Row, Column] = currentplayer;
                                            lastx = Column;
                                            lasty = Row;
                                            success = true;
                                            break;
                                        }
                                    }

                                    if (success)
                                    {

                                        //player1turn = false;

                                        //Update the message
                                        embed.Description = "";
                                        for (int r = 0; r < 6; r++)
                                        {
                                            if (r == 0)
                                            {
                                                for (int c = 0; c < 7; c++)
                                                {
                                                    embed.Description += $":{numlist[c]}:";
                                                }
                                                embed.Description += "\n";
                                            }
                                            for (int c = 0; c < 7; c++)
                                            {
                                                if (lines[r, c] == 0)
                                                {
                                                    embed.Description += $"{none}";
                                                }
                                                else if (lines[r, c] == 1)
                                                {
                                                    embed.Description += $"{blue}";
                                                }
                                                else if (lines[r, c] == 2)
                                                {
                                                    embed.Description += $"{red}";
                                                }
                                            }
                                            
                                            embed.Description += "\n";
                                        }
                                        embed.Description += "Usage:\n" +
                                                             "`connect4 [column]`\n" +
                                                             $":large_blue_circle: - {Context.User.Mention} {(currentplayer == 1 ? "" : "<-")}\n" +
                                                             $":red_circle: - {Context.Guild.GetUser(player2.userID)?.Mention} {(currentplayer == 2 ? "" : "<-")}";

                                        await game.ModifyAsync(x => x.Embed = embed.Build());

                                        //Check If it is a win here.
                                        var connectioncount = 0;
                                        //Checking Horizontally (Rows)
                                        for (int i = 0; i <= 6; i++)
                                        {

                                            if (lines[lasty, i] == currentplayer)
                                            {
                                                connectioncount++;
                                            }
                                            else
                                            {
                                                connectioncount = 0;
                                            }

                                            if (connectioncount >= 4)
                                            {
                                                //await ReplyAsync($"Player {currentplayer} Wins! Horizontal");
                                                playinggame = false;
                                                winmethod = "Horizontal";
                                                break;
                                            }
                                        }

                                        //Checking Vertically (Columns)
                                        connectioncount = 0;
                                        for (int i = 0; i <= 5; i++)
                                        {

                                            if (lines[i, lastx] == currentplayer)
                                            {
                                                connectioncount++;
                                            }
                                            else
                                            {
                                                connectioncount = 0;
                                            }

                                            if (connectioncount >= 4)
                                            {
                                                //await ReplyAsync($"Player {currentplayer} Wins! Vertical");
                                                playinggame = false;
                                                winmethod = "Vertical";
                                                break;
                                            }
                                        }

                                        /*     C    O    L    U    M    N    S                                      
                                           R [0,0][0,1][0,2][0,3][0,4][0,5][0,6]
                                           O [1,0][1,1][1,2][1,3][1,4][1,5][1,6]
                                           W [2,0][2,1][2,2][2,3][2,4][2,5][2,6]
                                           S [3,0][3,1][3,2][3,3][3,4][3,5][3,6]
                                             [4,0][4,1][4,2][4,3][4,4][4,5][4,6]
                                             [5,0][5,1][5,2][5,3][5,4][5,5][5,6]
                                         */


                                        //Checking Diagonally 
                                        int colinit, rowinit;
                                        //Top Left => Bottom Right (from top row diagonals)
                                        for (rowinit = 0; rowinit <= 5; rowinit++)
                                        {
                                            connectioncount = 0;
                                            int row, col;
                                            for (row = rowinit, col = 0; col <= 6 && row <= 5; col++, row++)
                                            {
                                                if (lines[row, col] == currentplayer)
                                                {
                                                    connectioncount++;
                                                    if (connectioncount >= 4)
                                                    {
                                                        //await ReplyAsync($"Player {currentplayer} Wins! Diagonal");
                                                        playinggame = false;
                                                        winmethod = "Diagonal";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    connectioncount = 0;
                                                }
                                            }
                                        }
                                        //Top Left => Bottom Right (from columns)
                                        for (colinit = 0; colinit <= 6; colinit++)
                                        {
                                            connectioncount = 0;
                                            int row, col;
                                            for (row = 0, col = colinit; col <= 6 && row <= 5; col++, row++)
                                            {
                                                if (lines[row, col] == currentplayer)
                                                {
                                                    connectioncount++;
                                                    if (connectioncount >= 2) Console.WriteLine($"R/C: {row}|{col} || P:{currentplayer} || CONN: {connectioncount} || D2");
                                                    if (connectioncount >= 4)
                                                    {
                                                        //await ReplyAsync($"Player {currentplayer} Wins! Diagonal");
                                                        playinggame = false;
                                                        winmethod = "Diagonal";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    connectioncount = 0;
                                                }
                                            }
                                        }
                                        //Checking other Diagonal.
                                        //Top Right => Bottom Left
                                        for (rowinit = 0; rowinit <= 5; rowinit++)
                                        {
                                            connectioncount = 0;
                                            int row, col;
                                            for (row = rowinit, col = 6; col >= 0 && row <= 5; col--, row++)
                                            {
                                                if (lines[row, col] == currentplayer)
                                                {
                                                    connectioncount++;
                                                    if (connectioncount >= 4)
                                                    {
                                                        playinggame = false;
                                                        winmethod = "Diagonal";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    connectioncount = 0;
                                                }
                                            }
                                        }

                                        for (colinit = 6; colinit >= 0; colinit--)
                                        {
                                            connectioncount = 0;
                                            int row, col;
                                            for (row = 0, col = colinit; col >= 0 && row <= 5; col--, row++)
                                            {
                                                if (lines[row, col] == currentplayer)
                                                {
                                                    connectioncount++;
                                                    if (connectioncount >= 4)
                                                    {
                                                        playinggame = false;
                                                        winmethod = "Diagonal";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    connectioncount = 0;
                                                }
                                            }

                                        }



                                        if (playinggame)
                                        {
                                            currentplayer = currentplayer == 1 ? 2 : 1;
                                            await next.DeleteAsync();
                                            msgtime = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                                            if (!embed.Description.Contains($"{none}"))
                                            {
                                                //This means all spaces are filles on the board
                                                //ie. a tie.

                                                await ReplyAsync(
                                                    "The Game is a draw. User Balances have not been modified. Good Game!");
                                                currentlobby.gamerunning = false;
                                                return;

                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        await ReplyAsync("Error, please specify a line that isn't full");
                                    }

                                }
                            }
                            else
                            {
                                await ReplyAsync("Invalid input, please try again in the format:\n" +
                                                 "`connect4 [line]`");
                            }
                        }


                    }
                    else
                    {
                        await ReplyAsync("Unknown Player/Not your turn.");
                    }
                }

                //await ReplyAsync($"{currentplayer} Wins.");
                var winner = currentplayer == 1 ? player1 : player2;
                var loser = currentplayer == 1 ? player2 : player1;
                await Connect4Win(winner.userID, loser.userID, bet, winmethod);


            }
            catch (Exception e)
            {
                await ReplyAsync(e.ToString());
                var currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
                if (currentlobby != null)
                {
                    currentlobby.gamerunning = false;
                }
            }

        }

        public async Task Connect4Win(ulong winnerID, ulong loserID, int bet, string winmethod)
        {
            var currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
            currentlobby.gamerunning = false;

            var finalguildobj = GuildConfig.GetServer(Context.Guild);
            var gwinner = finalguildobj.Gambling.Users.FirstOrDefault(x => x.userID == winnerID);
            var gloser = finalguildobj.Gambling.Users.FirstOrDefault(x => x.userID == loserID);
            gwinner.coins = gwinner.coins - bet;
            gloser.coins = gloser.coins - bet;
            var payout = bet * 2;
            gwinner.coins = gwinner.coins + payout;
            gwinner.totalbet = gwinner.totalbet + bet;
            gloser.totalbet = gloser.totalbet + bet;
            gwinner.totalpaidout = gwinner.totalpaidout + payout;
            IUser winner = Context.Guild.GetUser(gwinner.userID);
            IUser loser = Context.Guild.GetUser(gloser.userID);


            var embed2 = new EmbedBuilder
            {
                Title = $"{winner.Username} Wins!",
                Description = $"Winner: {winner.Username}\n" +
                              $"Coins: {gwinner.coins}\n" +
                              $"Payout: {payout}\n" +
                              $"WinLine: {winmethod}\n" +
                              $"Loser: {loser?.Username}\n" +
                              $"Coins: {gloser.coins}\n" +
                              $"Loss: {bet}"
            };
            await ReplyAsync("", false, embed2.Build());
            GuildConfig.SaveServer(finalguildobj);
        }

        /*
        [Command("Diagonal", RunMode = RunMode.Async)]
        [Summary("Diagonal")]
        [Remarks("Diagonal")]
        public async Task Diag(int mode = 2)
        {
            var lines = new int[6, 7];
            var embed = new EmbedBuilder();
            embed.Description = "";
            string none = ":black_circle:";
            string blue = ":large_blue_circle:";
            string red = ":red_circle:";
            var numlist = new List<string>
            {
                "zero",
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
                "nine"
            };
            var emsg = await ReplyAsync("", false, embed.Build());
            try
            {
                int connectioncount = 0;
                int colinit, rowinit;
                if (mode == 1)
                {
                    //Checking Diagonally 
                
                    //Top Left => Bottom Right (from top row diagonals)
                    for (rowinit = 0; rowinit <= 5; rowinit++)
                    {
                        connectioncount = 0;
                        int row, col;
                        for (row = rowinit, col = 0; col <= 6 && row <= 5; col++, row++)
                        {
                            if (lines[row, col] == 0)
                            {
                                lines[row, col] = 1;
                                connectioncount++;
                                if (connectioncount >= 4)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                connectioncount = 0;
                            }
                        }
                    }

                    //Top Left => Bottom Right (from columns)
                    for (colinit = 0; colinit <= 6; colinit++)
                    {
                        connectioncount = 0;
                        int row, col;
                        for (row = 0, col = colinit; col <= 6 && row <= 5; col++, row++)
                        {
                            if (lines[row, col] == 0)
                            {
                                lines[row, col] = 1;
                                connectioncount++;
                                if (connectioncount >= 4)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                connectioncount = 0;
                            }
                        }
                    }
                }
                else if (mode == 2)
                {
                    //Checking other Diagonal.
                    //Top Right => Bottom Left
                    for (rowinit = 0; rowinit <= 5; rowinit++)
                    {
                        connectioncount = 0;
                        int row, col;
                        for (row = rowinit, col = 6; col >= 0 && row <= 5; col--, row++)
                        {
                            if (lines[row, col] == 0)
                            {
                                lines[row, col] = 2;
                                connectioncount++;
                                if (connectioncount >= 4)
                                {
                                    //break;
                                }
                            }
                            else
                            {
                                connectioncount = 0;
                            }
                        }
                    }

                    for (colinit = 6; colinit >= 0; colinit--)
                    {
                        connectioncount = 0;
                        int row, col;
                        for (row = 0, col = colinit; col >= 0 && row <= 5; col--, row++)
                        {
                            if (lines[row, col] == 0)
                            {
                                lines[row, col] = 2;
                                connectioncount++;
                                if (connectioncount >= 4)
                                {
                                    //break;
                                }
                            }
                            else
                            {
                                connectioncount = 0;
                            }
                        }

                    }
                    //await emsg.ModifyAsync(x => x.Embed = embed.Build());
                }

            }
            catch (Exception e)
            {
                await ReplyAsync(e.ToString());
            }


        }
        */

        public Task Setupuser(IGuild guild, IUser user)
        {
            var guildobj = GuildConfig.GetServer(guild);
            var guser = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == user.Id);
            if (guser == null)
            {
                guildobj.Gambling.Users.Add(new GuildConfig.gambling.user
                {
                    userID = user.Id,
                    banned = false,
                    coins = 200
                });
                GuildConfig.SaveServer(guildobj);
            }

            return Task.CompletedTask;
        }
    }
}
