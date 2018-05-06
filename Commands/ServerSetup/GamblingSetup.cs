using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class GamblingSetup : InteractiveBase
    {
        [Command("setCurrencyName")]
        [Summary("setCurrencyName <name>")]
        [Remarks("set the name of the server's currency (ie. Coins)")]
        public async Task SCurrencyName([Remainder] string name = null)
        {
            var guildobj = GuildConfig.GetServer(Context.Guild);
            if (name == null)
            {
                await ReplyAsync("Currency Name has been set to default -> `Coins`");
                guildobj.Gambling.settings.CurrencyName = "Coins";
            }
            else
            {
                await ReplyAsync($"Currency Name has been set to -> `{name}`");
                guildobj.Gambling.settings.CurrencyName = name;
            }

            GuildConfig.SaveServer(guildobj);
        }

        [Group("Store")]
        public class Store : ModuleBase
        {
            [Command("SetupInfo")]
            [Summary("store SetupInfo")]
            [Remarks("help on setting up the store.")]
            public async Task StoreInfo()
            {
                await ReplyAsync("Add an item using the `additem` command, the format is as follows:\n" +
                                 $"`{Config.Load().Prefix}store AddItem <Price> <Quantity> <Item Name>`\n" +
                                 "Note for unlimited quantity, use `-1` as the value");
            }

            [Command("AddItem")]
            [Summary("store AddItem <Price> <Quantity> <Item Name>")]
            [Remarks("add an item to the servers default store")]
            public async Task AddStoreItem(int ItmCost, int ItmQuantity, [Remainder] string ItemName)
            {
                var guildobj = GuildConfig.GetServer(Context.Guild);
                var newitem = new GuildConfig.gambling.TheStore.Storeitem
                {
                    ItemName = ItemName,
                    cost = ItmCost,
                    quantity = ItmQuantity,
                    InitialCreatorID = Context.User.Id,
                    ItemID = guildobj.Gambling.Store.ShowItems.Count
                };

                guildobj.Gambling.Store.ShowItems.Add(newitem);
                GuildConfig.SaveServer(guildobj);
            }

            [Command("EditItem")]
            [Summary("store EditItem <Item Name>")]
            [Remarks("Edit an item in the store")]
            public async Task EditStoreItem([Remainder] string ItemName)
            {
                var guildobj = GuildConfig.GetServer(Context.Guild);

                var selecteditem = guildobj.Gambling.Store.ShowItems.Where(x => x.Hidden == false).FirstOrDefault(x =>
                    string.Equals(x.ItemName, ItemName, StringComparison.CurrentCultureIgnoreCase));
                if (selecteditem == null)
                {
                    await ReplyAsync("There are no items in the store with that name.");
                    return;
                }

                var embed = new EmbedBuilder();
                embed.Title = selecteditem.ItemName;
                embed.Description = $"`1` Attack: {selecteditem.Attack}\n" +
                                    $"`2` Defense: {selecteditem.Defense}\n" +
                                    $"`3` Cost: {selecteditem.cost}\n" +
                                    $"`4` Quantity: {selecteditem.quantity}\n" +
                                    $"`5` Total Purchased: {selecteditem.total_purchased}\n" +
                                    $"`6` Name: {selecteditem.ItemName}\n\n" +
                                    $"Reply with the option you would like to edit and the new value, eg.\n" +
                                    $"`1 50`\n" +
                                    $"The item's attack would be changed to 50.";


                GuildConfig.SaveServer(guildobj);
            }

            [Command("RemoveItem")]
            [Summary("store RemoveItem <Item Name>")]
            [Remarks("remove an item from the servers default store")]
            public async Task RemoveStoreItem([Remainder] string ItemName)
            {
                var guildobj = GuildConfig.GetServer(Context.Guild);

                var selecteditem = guildobj.Gambling.Store.ShowItems.FirstOrDefault(x =>
                    string.Equals(x.ItemName, ItemName, StringComparison.CurrentCultureIgnoreCase));
                if (selecteditem == null)
                {
                    await ReplyAsync("There are no items in the store with that name.");
                    return;
                }

                selecteditem.Hidden = true;
                GuildConfig.SaveServer(guildobj);
                await ReplyAsync($"{ItemName} has been removed from the store");
            }
        }
    }
}