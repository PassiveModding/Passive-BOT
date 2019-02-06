using System;
using System.Collections.Generic;

namespace PassiveBOT.Modules.Gaming
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Preconditions;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Services;

    [RequireContext(ContextType.Guild)]
    public partial class Games : Base
    {
        public Games(GameService gameService, HttpClient client)
        {
            GameService = gameService;
            Client = client;
        }

        public GameService GameService { get; set; }

        public HttpClient Client { get; set; }

        private readonly List<string> itemList = new List<string>
        {
            "💯", //:100:
            "🌻", //:sunflower:
            "🌑", //:new_moon:
            "🐠", //:tropical_fish: 
            "🎄", //:christmas_tree: 
            "👾", // space invaders
            "⚽" // soccer ball
        };

        [Command("DailyReward", RunMode = RunMode.Async)]
        [Summary("DailyReward")]
        [Remarks("Get 200 free coins for PassiveBOT Gambling")]
        [RateLimit(1, 1, Measure.Days)]
        public Task DailyRewardAsync()
        {
            var guildobj = GameService.GetServer(Context.Guild);
            var guser = guildobj.GetUser(Context.User);
            guser.Coins = guser.Coins + 200;

            var embed = new EmbedBuilder
            {
                Title = $"Success, you have received 200 {guildobj.Settings.CurrencyName}",
                Description = $"Balance: {guser.Coins} {guildobj.Settings.CurrencyName}",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };

            return ReplyAsync("", false, embed.Build());
        }

        [Command("GambleStats", RunMode = RunMode.Async)]
        [Summary("GambleStats")]
        [Remarks("Get a user's gambling stats")]
        public Task GambleStatsAsync(IUser user = null)
        {
            var guildobj = GameService.GetServer(Context.Guild);
            if (user == null)
            {
                user = Context.User;
            }

            var guser = guildobj.GetUser(user);

            var embed = new EmbedBuilder
            {
                Title = $"{user.Username} Gambling Stats",
                Description = $"Balance: {guser.Coins} {guildobj.Settings.CurrencyName}\n" +
                              $"Total Bet: {guser.TotalBet} {guildobj.Settings.CurrencyName}\n" +
                              $"Total Paid Out: {guser.TotalPaidOut} {guildobj.Settings.CurrencyName}\n",
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{user.Username}#{user.Discriminator}"
                }
            };

            return ReplyAsync("", false, embed.Build());
        }

        [Command("Slots", RunMode = RunMode.Async)]
        [Summary("Slots <bet>")]
        [Remarks("Play Slots")]
        public async Task Slots(int bet = 0)
        {
            var guildobj = GameService.GetServer(Context.Guild);

            // Initially we check wether or not the user is able to bet
            if (bet <= 0)
            {
                await ReplyAsync($"Please place a bet, ie. 10 {guildobj.Settings.CurrencyName}!");
                return;
            }

            var guser = guildobj.GetUser(Context.User);
            if (bet > guser.Coins)
            {
                await ReplyAsync($"Your bet is too high, please place a bet less than or equal to {guser.Coins}");
                return;
            }

            // now we deduct the bet amount from the users balance
            guser.Coins = guser.Coins - bet;

            var selections = new string[3];
            for (var i = 0; i < selections.Length; i++)
            {
                selections[i] = itemList[new Random().Next(0, itemList.Count)];
            }

            // Winning Combos
            // Three of any
            // 3, 2 or 1, :100:'s
            // 3 XMAS Trees
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

            guser.Coins = guser.Coins + payout;
            guser.TotalPaidOut = guser.TotalPaidOut + payout;
            guser.TotalBet = guser.TotalBet + bet;

            var embed = new EmbedBuilder
            {
                Title = "SLOTS",
                Description = $"➡️ {selections[0]}{selections[1]}{selections[2]} ⬅️\n\n" +
                              $"BET: {bet} {guildobj.Settings.CurrencyName}\n" +
                              $"PAY: {payout} {guildobj.Settings.CurrencyName}\n" +
                              $"BAL: {guser.Coins} {guildobj.Settings.CurrencyName}",
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}"
                }
            };
            await ReplyAsync("", false, embed.Build());
        }       
    }
}
