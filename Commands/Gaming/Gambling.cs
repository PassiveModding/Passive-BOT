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
                Title = $"Success, you have received 200 {guildobj.Gambling.settings.CurrencyName}",
                Description = $"Balance: {guser.coins} {guildobj.Gambling.settings.CurrencyName}",
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
        public async Task DailyReward(IUser user = null)
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
                Description = $"Balance: {guser.coins} {guildobj.Gambling.settings.CurrencyName}\n" +
                              $"Total Bet: {guser.totalbet} {guildobj.Gambling.settings.CurrencyName}\n" +
                              $"Total Paid Out: {guser.totalpaidout} {guildobj.Gambling.settings.CurrencyName}\n",
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{user.Username}#{user.Discriminator}"
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
            var guildobj = GuildConfig.GetServer(Context.Guild);
            //Initially we check wether or not the user is able to bet
            if (bet <= 0)
            {
                await ReplyAsync($"Please place a bet, ie. 10 {guildobj.Gambling.settings.CurrencyName}!");
                return;
            }
            
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
                              $"BET: {bet} {guildobj.Gambling.settings.CurrencyName}\n" +
                              $"PAY: {payout} {guildobj.Gambling.settings.CurrencyName}\n" +
                              $"BAL: {guser.coins} {guildobj.Gambling.settings.CurrencyName}",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };
            await ReplyAsync("", false, embed.Build());
        }

        /*
        [Command("DoubleOrNothing", RunMode = RunMode.Async)]
        [Summary("DoubleOrNothing <bet>")]
        [Remarks("50/50 chance to double your money")]
        public async Task Double(int bet = 0)
        {
            await Setupuser(Context.Guild, Context.User);
            //Initially we check wether or not the user is able to bet
            var guildobj = GuildConfig.GetServer(Context.Guild);
            if (bet <= 0)
            {
                await ReplyAsync($"Please place a bet, ie. 10 {guildobj.Gambling.settings.CurrencyName}!");
                return;
            }
            
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
                    Description = $"BET: {bet} {guildobj.Gambling.settings.CurrencyName}\n" +
                                  $"PAY: {payout} {guildobj.Gambling.settings.CurrencyName}\n" +
                                  $"BAL: {guser.coins} {guildobj.Gambling.settings.CurrencyName}",
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
                    Description = $"BET: {bet} {guildobj.Gambling.settings.CurrencyName}\n" +
                                  $"PAY: {payout} {guildobj.Gambling.settings.CurrencyName}\n" +
                                  $"BAL: {guser.coins} {guildobj.Gambling.settings.CurrencyName}",
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

        }*/

        [Command("Connect4", RunMode = RunMode.Async)]
        [Summary("Connect4 <bet>")]
        [Remarks("Play connect 4 with another person")]
        public async Task Connect4(int bet = 0)
        {
            //I am wrapping this in a try catch until I finish finding all the bugs n shit.
            try
            {
                //Here we check wether or not there is currently a game running the the current lobby as there is no simple way to differentiate games at the moment (will add later)
                var currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
                if (currentlobby != null)
                {
                    //We quit if there is a game already running
                    if (currentlobby.gamerunning)
                    {
                        await ReplyAsync("A game of connect4 is already running in this channel. Please wait until it is completed.");
                        return;
                    }
                    else
                    {
                        currentlobby.gamerunning = true;
                    }

                }
                else
                {
                    //Add a lobby in the case that there hasn't been a game played in the cureent one yet.
                    CommandHandler.Connect4List.Add(new CommandHandler.Con4GameList
                    {
                        channelID = Context.Channel.Id,
                        gamerunning = true
                    });
                    currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
                }
                await Setupuser(Context.Guild, Context.User);
                //Filter out invalid bets to make sure that games are played fairly
                var initguildobj = GuildConfig.GetServer(Context.Guild);
                if (bet <= 0)
                {
                    //Ensure there is no negative or zero bet games.
                    await ReplyAsync($"Please place a bet, ie. 10 {initguildobj.Gambling.settings.CurrencyName}!");
                    currentlobby.gamerunning = false;
                    return;
                }

                //Here get the the player 1's profile and ensure they are able to bet
                
                var initplayer1 = initguildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
                if (bet > initplayer1.coins)
                {
                    await ReplyAsync($"Your bet is too high, please place a bet less than or equal to {initplayer1.coins}");
                    currentlobby.gamerunning = false;
                    return;
                }

                GuildConfig.gambling.user p2 = null;


                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Connect4 Game (BET = {bet} {initguildobj.Gambling.settings.CurrencyName})",
                    Description = "Get somebody to type `connect4 accept` to start"
                });

                var accepted = false;
                var timeoutattempts = 0;
                var messagewashout = 0;
                //here we wait until another player accepts the game
                while (!accepted)
                {
                    var next = await NextMessageAsync(false, true, TimeSpan.FromSeconds(10));
                    if (next?.Author.Id == Context.User.Id)
                    {
                        //Ignore author messages for the game until another player accepts
                    }
                    else if (string.Equals(next?.Content, "connect4 accept", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await Setupuser(Context.Guild, next.Author);
                        var guildobj2 = GuildConfig.GetServer(Context.Guild);
                        p2 = guildobj2.Gambling.Users.FirstOrDefault(x => x.userID == next.Author.Id);
                        if (p2.coins < bet)
                        {
                            await ReplyAsync(
                                $"{next.Author.Mention} - You do not have enough {initguildobj.Gambling.settings.CurrencyName} to play this game, you need a minimum of {bet}. Your balance is {p2.coins} {initguildobj.Gambling.settings.CurrencyName}");
                        }
                        else
                        {
                            accepted = true;
                            p2 = guildobj2.Gambling.Users.FirstOrDefault(x => x.userID == next.Author.Id);
                        }
                    }

                    //Overload for is a message is not sent within the timeout
                    if (next == null)
                    {
                        //if more than 6 timeouts (1 minute of no messages) occur without a user accepting, we quit the game
                        timeoutattempts++;
                        if (timeoutattempts < 6) continue;
                        await ReplyAsync("Connect4: Timed out!");
                        currentlobby.gamerunning = false;
                        return;
                    }

                    //in case people are talking over the game, we also make the game quit after 25 messages are sent so it is not waiting indefinitely for a player to accept.
                    messagewashout++;
                    if (messagewashout <= 25) continue;
                    await ReplyAsync("Connect4: Timed out!");
                    currentlobby.gamerunning = false;
                    return;
                }

                var initgameguildobj = GuildConfig.GetServer(Context.Guild);
                var player1 = initgameguildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
                var player2 = initgameguildobj.Gambling.Users.FirstOrDefault(x => x.userID == p2.userID);

                var lines = new int[6, 7];

                const string none = ":black_circle:";
                const string blue = ":large_blue_circle:";
                const string red = ":red_circle:";
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
                var embed = new EmbedBuilder();

                //Here we build the initial game board with all empty squares
                for (var r = 0; r < 6; r++)
                {
                    if (r == 0)
                    {
                        for (var c = 0; c < 7; c++)
                        {
                            embed.Description += $":{numlist[c]}:";
                        }
                        embed.Description += "\n";
                    }
                    for (var c = 0; c < 7; c++)
                    {
                        embed.Description += $"{none}";
                    }

                    embed.Description += "\n";
                }

                embed.Description += "Usage:\n" +
                                     "`connect4 [column]`\n" +
                                     $":large_blue_circle: - {Context.User.Mention} <-\n" +
                                     $":red_circle: - {Context.Guild.GetUser(player2.userID)?.Mention}";
                embed.Footer = new EmbedFooterBuilder
                {
                  Text = $"it is {Context.User.Username}'s turn"
                };

                var gamemessage = await ReplyAsync("", false, embed.Build());

                //LastX and LastY are used to check for horizontal and vertical wins
                var lastx = 0;
                var lasty = 0;
                var playinggame = true;
                //We always begin the game with whoever ran the initial command.
                var currentplayer = 1;
                var winmethod = "";
                //MSGTime is used to ensure that only a single minute passes inbetween turns, if it goes past a minute between turns then we count it as a player forfitting.
                var msgtime = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                var errormsgs = "";
                while (playinggame)
                {
                    var errormsgs1 = errormsgs;
                    if (!string.IsNullOrEmpty(errormsgs1))
                    {
                        await gamemessage.ModifyAsync(x => x.Content = errormsgs1);
                    }
                    else
                    {
                        await gamemessage.ModifyAsync(x => x.Content = " ");
                    }

                    errormsgs = "";
                    //Using InteractiveBase we wait until a message is sent in the current game
                    var next = await NextMessageAsync(false, true, TimeSpan.FromMinutes(1));
                    
                    //If the player doesn't show up mark them as forfeitting and award a win to the other player.
                    if (next == null || msgtime < DateTime.UtcNow)
                    {
                        await ReplyAsync(
                            $"{(currentplayer == 1 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)} Did not reply fast enough. Auto forfitting");

                        var w = currentplayer == 1 ? player2.userID : player1.userID;
                        var l = currentplayer == 1 ? player1.userID : player2.userID;
                        await Connect4Win(w,l, bet, "Player Forfitted.");
                        return;
                    }

                    //filter out non game messages by ignoring ones
                    if (!next.Content.ToLower().StartsWith("connect4")) continue;

                    //Ensure that we only accept messages from players that are in the game
                    if (next.Author.Id != player1.userID && next.Author.Id != player2.userID)
                    {
                        //await ReplyAsync("You are not part of this game.");
                        errormsgs = $"{next.Author.Mention} You are not part of this game.";
                        await next.DeleteAsync();
                        continue;
                    }

                    //Ensure that the current message is from a player AND it is also their turn.
                    if ((next.Author.Id == player1.userID && currentplayer == 1) || (next.Author.Id == player2.userID && currentplayer == 2))
                    {
                        //filter out invalid line submissions
                        var parameters = next.Content.Split(" ");
                        //Make sure that the message is in the correct format of connect4 [line]
                        if (parameters.Length != 2 || !int.TryParse(parameters[1], out int Column))
                        {
                            errormsgs = $"{(currentplayer == 2 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)} \n" +
                                        $"Invalid Line input, here is an example input:\n" +
                                             "`connect4 3` - this will place a counter in line 3.\n" +
                                             "NOTE: Do not use the bot's prefix, just write `connect4 [line]`";
                            await next.DeleteAsync();
                            continue;
                        }

                        //as there are only 7 columns to pick from, filter out values outside of this range.
                        if (Column < 0 || Column > 6)
                        {
                            //error invalid line.
                            errormsgs = $"{(currentplayer == 2 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)}\n" +
                                        $"Invalid input, line number must be from 0-6 message in the format:\n" +
                                "`connect4 [line]`";
                            await next.DeleteAsync();
                            continue;
                        }

                        bool success = false;
                        //moving from the top of the board downwards
                        for (var Row = 5; Row >= 0; Row--)
                        {
                            if (lines[Row, Column] != 0) continue;
                            lines[Row, Column] = currentplayer;
                            lastx = Column;
                            lasty = Row;
                            success = true;
                            break;
                        }

                        //Ensure that we only move to the next player's turn IF the current player actually makes a move in an available column.
                        if (!success)
                        {
                            errormsgs = $"{(currentplayer == 2 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)}\n" +
                                        $"Error, please specify a line that isn't full";
                            await next.DeleteAsync();
                            continue;
                        }

                        //Update the embed message
                        embed.Description = "";
                        for (var r = 0; r < 6; r++)
                        {
                            if (r == 0)
                            {
                                for (var c = 0; c < 7; c++)
                                {
                                    embed.Description += $":{numlist[c]}:";
                                }

                                embed.Description += "\n";
                            }

                            for (var c = 0; c < 7; c++)
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
                        embed.Footer = new EmbedFooterBuilder
                        {
                            Text = $"it is {(currentplayer == 2 ? Context.Guild.GetUser(player1.userID)?.Username : Context.Guild.GetUser(player2.userID)?.Username)}'s turn"
                        };
                        await gamemessage.ModifyAsync(x => x.Embed = embed.Build());

                        //Check If it is a win here.
                        var connectioncount = 0;
                        //Checking Horizontally (Rows)
                        for (var i = 0; i <= 6; i++)
                        {
                            if (lines[lasty, i] == currentplayer)
                            {
                                connectioncount++;
                            }
                            else
                            {
                                connectioncount = 0;
                            }

                            if (connectioncount < 4) continue;
                            //await ReplyAsync($"Player {currentplayer} Wins! Horizontal");
                            playinggame = false;
                            winmethod = "Horizontal";
                            break;
                        }

                        //Checking Vertically (Columns)
                        connectioncount = 0;
                        for (var i = 0; i <= 5; i++)
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
                                    if (connectioncount < 4) continue;
                                    playinggame = false;
                                    winmethod = "Diagonal";
                                    break;
                                }

                                connectioncount = 0;
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
                                    if (connectioncount < 4) continue;
                                    playinggame = false;
                                    winmethod = "Diagonal";
                                    break;
                                }

                                connectioncount = 0;
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
                                    if (connectioncount < 4) continue;
                                    playinggame = false;
                                    winmethod = "Diagonal";
                                    break;
                                }

                                connectioncount = 0;
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
                                    if (connectioncount < 4) continue;
                                    playinggame = false;
                                    winmethod = "Diagonal";
                                    break;
                                }

                                connectioncount = 0;
                            }
                        }

                        //If we have a win, do dont switch the current player.
                        if (!playinggame) continue;

                        currentplayer = currentplayer == 1 ? 2 : 1;
                        //To reduce the amount of messages after the game, delete the connect4 message.
                        await next.DeleteAsync();
                        msgtime = DateTime.UtcNow + TimeSpan.FromMinutes(1);

                        if (!embed.Description.Contains($"{none}"))
                        {
                            //This means all spaces are filled on the board
                            //ie. a tie.

                            await ReplyAsync(
                                "The Game is a draw. User Balances have not been modified. Good Game!");
                            currentlobby.gamerunning = false;
                            return;
                        }
                    }
                    else
                    {
                        errormsgs = $"{(currentplayer == 2 ? Context.Guild.GetUser(player1.userID)?.Mention : Context.Guild.GetUser(player2.userID)?.Mention)}\n" +
                                    "Unknown Player/Not your turn.";
                        await next.DeleteAsync();
                    }
                }

                //Finally now that the game is finished, go and modify user's scores.
                var winner = currentplayer == 1 ? player1 : player2;
                var loser = currentplayer == 1 ? player2 : player1;
                await Connect4Win(winner.userID, loser.userID, bet, winmethod);
            }
            catch (Exception e)
            {
                //If there is an error, we need to ensure that the current channel can still initiate new games.
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
            //make sure we allow other connect4 games to be played in the current channel now.
            var currentlobby = CommandHandler.Connect4List.FirstOrDefault(x => x.channelID == Context.Channel.Id);
            currentlobby.gamerunning = false;

            //Get the users and modify their scores and stats.
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
                              $"{finalguildobj.Gambling.settings.CurrencyName}: {gwinner.coins} \n" +
                              $"Payout: {payout}\n" +
                              $"WinLine: {winmethod}\n" +
                              $"Loser: {loser?.Username}\n" +
                              $"{finalguildobj.Gambling.settings.CurrencyName}: {gloser.coins}\n" +
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
            if (guildobj.Gambling.Users.FirstOrDefault(x => x.userID == user.Id) == null)
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
