using System;
using System.Collections.Generic;
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


        [Group("StoreSetup")]
        public class Store : InteractiveBase
        {
            [Command("SetupInfo")]
            [Summary("StoreSetup SetupInfo")]
            [Remarks("help on setting up the store.")]
            public async Task StoreInfo()
            {
                await ReplyAsync("Add an item using the `additem` command, the format is as follows:\n" +
                                 $"`{Config.Load().Prefix}store AddItem <Price> <Quantity> <Item Name>`\n" +
                                 "Note for unlimited quantity, use `-1` as the value");
            }

            [Command("InitialiseStore")]
            [Summary("StoreSetup InitialiseStore <Price> <Quantity> <Item Name>")]
            [Remarks("initialise the store!")]
            public async Task InitStore()
            {
                var guildobj = GuildConfig.GetServer(Context.Guild);
                var sitems = new List<GuildConfig.gambling.TheStore.Storeitem>
                {
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":wrench:",
                        ItemID = -15,
                        Hidden = true,
                        cost = 15
                    },
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":evergreen_tree:",
                        ItemID = -10,
                        Hidden = true,
                        cost = 10
                    },
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":full_moon:",
                        ItemID = -5,
                        Hidden = true,
                        cost = 12
                    },
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":zap:",
                        ItemID = -16,
                        Hidden = true,
                        cost = 50
                    },
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":apple:",
                        ItemID = -11,
                        Hidden = true,
                        cost = 25
                    },
                    new GuildConfig.gambling.TheStore.Storeitem
                    {
                        ItemName = ":gem:",
                        ItemID = -6,
                        Hidden = true,
                        cost = 100
                    }
                };
                guildobj.Gambling.Store.ShowItems.AddRange(sitems);
                GuildConfig.SaveServer(guildobj);
                await ReplyAsync("Added");
            }

            [Command("AddItem")]
            [Summary("StoreSetup AddItem <Price> <Quantity> <Item Name>")]
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
                var embed = new EmbedBuilder();
                embed.Title = ItemName + " Added";
                embed.Description = $"`1` Attack: {newitem.Attack}\n" +
                                    $"`2` Defense: {newitem.Defense}\n" +
                                    $"`3` Cost: {newitem.cost}\n" +
                                    $"`4` Quantity: {newitem.quantity}\n" +
                                    $"`5` Total Purchased: {newitem.total_purchased}\n" +
                                    $"`6` Name: {newitem.ItemName}\n";
                await ReplyAsync("", false, embed.Build());
                guildobj.Gambling.Store.ShowItems.Add(newitem);
                GuildConfig.SaveServer(guildobj);
            }

            [Command("EditItem", RunMode = RunMode.Async)]
            [Summary("StoreSetup EditItem <Item Name>")]
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

                var next = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1));
                var paramlist = next.Content.Split(' ');
                var inputnumber = paramlist[0];
                var inputvalue = next.Content.Substring(inputnumber.Length, next.Content.Length - inputnumber.Length);
                var editlog = "";
                if (int.TryParse(inputnumber, out var selectionResult))
                {
                    if (selectionResult == 1)
                        if (int.TryParse(inputvalue, out var EditValue))
                        {
                            editlog = $"Attack Value: {selecteditem.Attack} -> {EditValue}";
                            selecteditem.Attack = EditValue;
                        }
                        else
                        {
                            await ReplyAsync("Input 1 detected, invalid edit value");
                            return;
                        }
                    else if (selectionResult == 2)
                        if (int.TryParse(inputvalue, out var EditValue))
                        {
                            editlog = $"Defense Value: {selecteditem.Defense} -> {EditValue}";
                            selecteditem.Defense = EditValue;
                        }
                        else
                        {
                            await ReplyAsync("Input 2 detected, invalid edit value");
                            return;
                        }
                    else if (selectionResult == 3)
                        if (int.TryParse(inputvalue, out var EditValue))
                        {
                            editlog = $"Cost: {selecteditem.cost} -> {EditValue}";
                            selecteditem.cost = EditValue;
                        }
                        else
                        {
                            await ReplyAsync("Input 3 detected, invalid edit value");
                            return;
                        }
                    else if (selectionResult == 4)
                        if (int.TryParse(inputvalue, out var EditValue))
                        {
                            editlog = $"Quantity: {selecteditem.quantity} -> {EditValue}";
                            selecteditem.quantity = EditValue;
                        }
                        else
                        {
                            await ReplyAsync("Input 4 detected, invalid edit value");
                            return;
                        }
                    else if (selectionResult == 5)
                        if (int.TryParse(inputvalue, out var EditValue))
                        {
                            editlog = $"Total Purchased: {selecteditem.total_purchased} -> {EditValue}";
                            selecteditem.total_purchased = EditValue;
                        }
                        else
                        {
                            await ReplyAsync("Input 5 detected, invalid edit value");
                            return;
                        }
                    else if (selectionResult == 6)
                        selecteditem.ItemName = inputvalue;
                }
                else
                {
                    await ReplyAsync("Invalid Input. please input just a number and the desired new value");
                }

                await ReplyAsync($"{editlog}");
                GuildConfig.SaveServer(guildobj);
            }

            [Command("RemoveItem")]
            [Summary("StoreSetup RemoveItem <Item Name>")]
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

                if (selecteditem.ItemID < 0)
                {
                    await ReplyAsync("You cannot remove the default items");
                    return;
                }

                selecteditem.Hidden = true;
                GuildConfig.SaveServer(guildobj);
                await ReplyAsync($"{ItemName} has been removed from the store");
            }
        }
    }
}