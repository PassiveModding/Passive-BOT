using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands.Currency
{
    [RequireContext(ContextType.Guild)]
    [Ratelimit(1, 1d, Measure.Minutes)]
    public class Earnings : ModuleBase
    {
        //Rare
        //:apple: :gem: :zap: 
        //Defualt
        //:evergreen_tree: :full_moon: :wrench: 

        [Command("Mine")]
        [Summary("Mine")]
        [Remarks("Mine some Stone and earn!")]
        public async Task MineTask()
        {
            var rnd = new Random();
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            var rvalue = rnd.Next(1, 8);
            var rreward = rnd.Next(0, 10);
            var CheckStone = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -5);
            if (CheckStone == null)
                uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                {
                    ItemID = -5,
                    quantity = rvalue
                });
            else
                CheckStone.quantity = CheckStone.quantity + rvalue;

            var rewardstr = "";
            if (rreward == 9)
            {
                var CheckGem = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -6);
                if (CheckGem == null)
                    uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                    {
                        ItemID = -6,
                        quantity = 1
                    });
                else
                    CheckGem.quantity++;

                rewardstr = $"Congratulations, you also mined: 1 Gem! :gem:";
            }

            await ReplyAsync($":full_moon: You Mined {rvalue} Stone!\n{rewardstr}");
            GuildConfig.SaveServer(guildobj);
        }

        [Command("WoodCut")]
        [Summary("WoodCut")]
        [Remarks("Cut some Trees and earn!")]
        public async Task WoodCut()
        {
            var rnd = new Random();
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            var rvalue = rnd.Next(1, 10);
            var rreward = rnd.Next(0, 10);
            var CheckWood = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -10);
            if (CheckWood == null)
                uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                {
                    ItemID = -10,
                    quantity = rvalue
                });
            else
                CheckWood.quantity = CheckWood.quantity + rvalue;

            var rewardstr = "";
            if (rreward == 9)
            {
                var CheckApple = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -11);
                if (CheckApple == null)
                    uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                    {
                        ItemID = -11,
                        quantity = 1
                    });
                else
                    CheckApple.quantity++;

                rewardstr = $"Congratulations, you also Picked: 1 Apple! :apple:";
            }

            await ReplyAsync($":evergreen_tree: You Cut {rvalue} Logs!\n{rewardstr}");
            GuildConfig.SaveServer(guildobj);
        }

        [Command("Mechanic")]
        [Summary("Mechanic")]
        [Remarks("Work as a mechanic and earn!")]
        public async Task Mechanic()
        {
            var rnd = new Random();
            var guildobj = GuildConfig.GetServer(Context.Guild);
            var uprofile = guildobj.Gambling.Users.FirstOrDefault(x => x.userID == Context.User.Id);
            var rvalue = rnd.Next(1, 6);
            var rreward = rnd.Next(0, 10);
            var CheckWrench = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -15);
            if (CheckWrench == null)
                uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                {
                    ItemID = -15,
                    quantity = rvalue
                });
            else
                CheckWrench.quantity = CheckWrench.quantity + rvalue;

            var rewardstr = "";
            if (rreward == 9)
            {
                var CheckPPack = uprofile.Inventory.FirstOrDefault(x => x.ItemID == -16);
                if (CheckPPack == null)
                    uprofile.Inventory.Add(new GuildConfig.gambling.user.item
                    {
                        ItemID = -16,
                        quantity = 1
                    });
                else
                    CheckPPack.quantity++;

                rewardstr = $"Congratulations, you also found a Power Pack! :zap:";
            }

            await ReplyAsync($":wrench: You Earned {rvalue} Wrenches!\n{rewardstr}");
            GuildConfig.SaveServer(guildobj);
        }
    }
}