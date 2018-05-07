using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands.Gaming
{
    [RequireContext(ContextType.Guild)]
    public class Store : ModuleBase
    {
        [Command("Inventory")]
        [Summary("Inventory <@user>")]
        [Remarks("view a user's inventory")]
        public async Task ViewInventory(IUser user = null)
        {
            await Setupuser(Context.Guild, Context.User);
            if (user == null) user = Context.User;

            var guildobj = GuildConfig.GetServer(Context.Guild);
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == user.Id);

            if (uprofile == null)
            {
                await ReplyAsync("User has not been set up yet.");
                return;
            }

            if (!uprofile.Inventory.Any())
            {
                await ReplyAsync("User does not own anything.");
                return;
            }

            var embed = new EmbedBuilder();
            var uitems = uprofile.Inventory
                .Select(x => guildobj.Gambling.Store.ShowItems.FirstOrDefault(s => s.ItemID == x.ItemID))
                .Where(x => x != null);
            foreach (var item in uitems)
            {
                var uitem = uprofile.Inventory.First(x => x.ItemID == item.ItemID);
                embed.AddField(item.ItemName, $"Quantity: {uitem.quantity}\n" +
                                              $"{(item.HasDurability ? $"Durability: {uitem.Durability}\n" : "")}" +
                                              $"Value: {item.cost}");
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("BrowseStore")]
        [Summary("BrowseStore")]
        [Remarks("Browse the Store")]
        public async Task ViewStore()
        {
            var guildobj = GuildConfig.GetServer(Context.Guild);

            var embed = new EmbedBuilder();
            foreach (var item in guildobj.Gambling.Store.ShowItems) //.Where(x => x.Hidden == false))
            {
                var desc = $"Cost: {item.cost}\n" +
                           $"Quantity In Stock: {item.quantity}\n" +
                           $"ID: {item.ItemID}\n" +
                           $"All time purchased: {item.total_purchased}\n";

                if (item.HasDurability)
                    desc += $"Attack: {item.Attack}\n" +
                            $"Defence: {item.Defense}\n" +
                            $"Durability: {item.Durability}\n" +
                            $"DurabilityModifier: {item.DurabilityModifier}\n";
                embed.AddField(item.ItemName, desc);
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Buy")]
        [Summary("Buy <ItemName>")]
        [Remarks("purchase an item from the store")]
        public async Task Purchase([Remainder] string name = null)
        {
            await Purchase(1, name);
        }

        [Command("Buy")]
        [Summary("Buy <quantity> <ItemName>")]
        [Remarks("purchase items item from the store")]
        public async Task Purchase(int quantity, [Remainder] string name = null)
        {
            await Setupuser(Context.Guild, Context.User);
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var selecteditem = guildobj.Gambling.Store.ShowItems.Where(x => x.Hidden == false).FirstOrDefault(x =>
                string.Equals(name, x.ItemName, StringComparison.InvariantCultureIgnoreCase));
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            if (selecteditem == null)
            {
                await ReplyAsync($"{name} is not an item im the store.");
                return;
            }

            if (uprofile.coins - selecteditem.cost * quantity < 0)
            {
                await ReplyAsync(
                    $"Insufficient Funds. This item costs {selecteditem.cost} {guildobj.Gambling.settings.CurrencyName}");
                return;
            }

            if (selecteditem.quantity == 0)
            {
                await ReplyAsync($"{selecteditem.ItemName} is not in stock at the moment");
                return;
            }

            if (selecteditem.quantity - quantity < 0)
            {
                await ReplyAsync($"There are not enough of this item in stock to buy that many at the moment.");
                return;
            }

            var invitem = uprofile.Inventory.FirstOrDefault(x => x.ItemID == selecteditem.ItemID);
            if (invitem == null || selecteditem.HasDurability || invitem.Durability < 100)
                uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                {
                    ItemID = selecteditem.ItemID,
                    quantity = quantity,
                    Durability = selecteditem.Durability
                });
            else
                invitem.quantity = invitem.quantity + quantity;

            uprofile.coins = uprofile.coins - selecteditem.cost * quantity;
            selecteditem.total_purchased = selecteditem.total_purchased + quantity;
            if (selecteditem.quantity != -1) selecteditem.quantity = selecteditem.quantity - quantity;
            await ReplyAsync($"You have successfully purchased `{quantity}` {selecteditem.ItemName}\n" +
                             $"Balance: {uprofile.coins} {guildobj.Gambling.settings.CurrencyName}");
            GuildConfig.SaveServer(guildobj);
        }

        [Command("Sell")]
        [Summary("Sell <ItemName>")]
        [Remarks("sell items to the store")]
        public async Task SellItem([Remainder] string name = null)
        {
            await SellItem(1, name);
        }

        [Command("Sell")]
        [Summary("Sell <quantity> <ItemName>")]
        [Remarks("sell items to the store")]
        public async Task SellItem(int quantity, [Remainder] string name = null)
        {
            await Setupuser(Context.Guild, Context.User);
            var guildobj = GuildConfig.GetServer(Context.Guild);

            var selecteditem = guildobj.Gambling.Store.ShowItems.FirstOrDefault(x =>
                string.Equals(name, x.ItemName, StringComparison.InvariantCultureIgnoreCase));
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            if (selecteditem == null)
            {
                await ReplyAsync($"{name} is not an item i the store.");
                return;
            }

            var invitem = uprofile.Inventory.Where(x => x.Durability == 100)
                .FirstOrDefault(x => x.ItemID == selecteditem.ItemID);
            if (invitem == null)
            {
                await ReplyAsync(
                    $"You do not own this item (or you have already used it and it's durability has decreased.)");
                return;
            }

            if (invitem.quantity < quantity)
            {
                await ReplyAsync($"You cannot sell this many of that item");
                return;
            }

            if (invitem.quantity - quantity == 0)
                uprofile.Inventory.Remove(invitem);
            else
                invitem.quantity = invitem.quantity - quantity;

            uprofile.coins = uprofile.coins + selecteditem.cost * quantity;
            if (selecteditem.quantity != -1) selecteditem.quantity = selecteditem.quantity + quantity;
            GuildConfig.SaveServer(guildobj);
        }

        [Command("QuickSell")]
        [Summary("QuickSell")]
        [Remarks("sell all stone, wood, wrenches, gems, apples and powerpacks to the store")]
        public async Task QuickSell()
        {
            await Setupuser(Context.Guild, Context.User);
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            var stone = uprofile.Inventory.Where(x => x.ItemID == -5).Sum(x => x.quantity) * 12;
            var wrenches = uprofile.Inventory.Where(x => x.ItemID == -15).Sum(x => x.quantity) * 15;
            var wood = uprofile.Inventory.Where(x => x.ItemID == -10).Sum(x => x.quantity) * 10;
            var power = uprofile.Inventory.Where(x => x.ItemID == -16).Sum(x => x.quantity) * 50;
            var apples = uprofile.Inventory.Where(x => x.ItemID == -11).Sum(x => x.quantity) * 25;
            var gems = uprofile.Inventory.Where(x => x.ItemID == -6).Sum(x => x.quantity) * 100;

            uprofile.coins = uprofile.coins + stone + wrenches + wood + power + apples + gems;
            uprofile.Inventory = uprofile.Inventory.Where(x =>
                x.ItemID != -5 && x.ItemID != -15 && x.ItemID != -10 && x.ItemID != -6 && x.ItemID != -16 &&
                x.ItemID != -11).ToList();
            await ReplyAsync($"Gained: {stone + wrenches + wood + power + apples + gems}\n" +
                             $"Balance: {uprofile.coins}");
            GuildConfig.SaveServer(guildobj);
        }

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