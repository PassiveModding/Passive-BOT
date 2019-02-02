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