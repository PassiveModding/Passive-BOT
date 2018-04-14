using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.preconditions;
using PassiveBOT.strings;

namespace PassiveBOT.Commands.Text
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Fun : ModuleBase
    {
        /*
        [Command("GetUpvoters")]
        [Summary("GetUpvoters")]
        [Remarks("Get a list of people who have upvoted the bot recently!")]
        public async Task GetUpvoters()
        {
            if (Config.Load().DBLtoken == null)
            {
                await ReplyAsync("Bot Not Configured for DiscordBots.org");
                return;
            }
            try
            {
                var DblApi = new DiscordNetDblApi(Context.Client, Config.Load().DBLtoken);
                var users = DblApi.GetVotersAsync(1).Result;
                var embed = new EmbedBuilder();
                var desc = "";
                foreach (var user in users)
                {
                    desc += $"{user.Username} <{user.Id}>\n";
                }

                desc += "You Can go vote for this bot here:\n" +
                        $"{Load.DBLLink}\n" +
                        $"Type: `{Config.Load().Prefix} Claim` for a prize once you've voted.";
                embed.Description = desc;
                embed.Title = "Voters for the Last 24H";
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("Claim")]
        [Summary("Claim")]
        [Remarks("Claim a prize for getting voting!")]
        public async Task ClaimPrize()
        {
            if (Config.Load().DBLtoken == null)
            {
                await ReplyAsync("Bot Not Configured for DiscordBots.org");
                return;
            }
            try
            {
                var DblApi = new DiscordNetDblApi(Context.Client, Config.Load().DBLtoken);
                var users = DblApi.GetVoterIdsAsync(1).Result;
                
                if (users.Contains(Context.User.Id))
                {
                    var embed = new EmbedBuilder {Title = "Prize Claimed", Color = Color.Green};
                    var rnd = new Random().Next(0, 2);
                    var desc = "";
                    switch (rnd)
                    {
                        case 0:
                            desc = "REEEEEEE You won nothing :/ Better luck tomorrow";
                            break;
                        case 1:
                            desc = "You Get a sexy crown! :crown:";
                            await ((IGuildUser)Context.User).ModifyAsync(x =>
                               x.Nickname =
                                   $":crown: {(x.Nickname.IsSpecified ? x.Nickname : Context.User.Username)}");
                            break;
                        case 2:
                            desc = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                            break;
                        default:
                            desc = "REEEEEEE You won nothing :/ Better luck tomorrow";
                            break;
                    }

                    embed.Description = desc;
                    await ReplyAsync("", false, embed.Build());
                }

            }
            catch
            {
                //
            }
        }*/

        [Command("Ping", RunMode = RunMode.Async)]
        [Alias("pong")]
        [Summary("ping")]
        [Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            if (Context.Client is DiscordSocketClient client)
            {
                var embed = new EmbedBuilder
                {
                    Title = "🏓 PassiveBOT 🏓",
                    Description = $"**Server Speed:** {client.Latency} ms\n" +
                                  "Wow, super fast!\n" +
                                  "PassiveBOT is amazing isnt it?",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                };
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("8ball")]
        [Summary("8ball <question?>")]
        [Remarks("ask me anything")]
        public async Task Ball([Remainder] [Optional] string input)
        {
            if (input == null)
            {
                await ReplyAsync($"Ask me a question silly, eg. `{Config.Load().Prefix} 8ball am I special?`");
            }
            else
            {
                var rnd = new Random();
                var embed = new EmbedBuilder
                {
                    Title = ":crystal_ball: PassiveBOT the bot that knows all :crystal_ball:",
                    Description = $"❓ {input}\n 🎱 {FunStr.Answers[rnd.Next(0, FunStr.Answers.Length)]}",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                };

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("fortune")]
        [Summary("fortune")]
        [Remarks("open a fortune cookie")]
        public async Task Fortune()
        {
            var rnd = new Random();
            var embed = new EmbedBuilder
            {
                Title = ":crystal_ball: PassiveBOT the Gypsy :crystal_ball:",
                Description = $"{FunStr.Fortune[rnd.Next(0, FunStr.Answers.Length)]}",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("rps")]
        [Summary("rps <r, p or s>")]
        [Remarks("rock paper scissors!")]
        public async Task Rps([Optional] string input)
        {
            if (input == null)
            {
                var pre = Config.Load().Prefix;
                await ReplyAsync(
                    "❓ to play rock, paper, scissors" +
                    $"\n\n:waning_gibbous_moon: type `{pre}rps rock` or `.{pre}ps r` to pick rock" +
                    $"\n\n:newspaper: type `{pre}rps paper` or `{pre}rps p` to pick paper" +
                    $"\n\n✂️ type `{pre}rps scissors` or `{pre}rps s` to pick scissors"
                );
            }
            else
            {
                int pick;
                switch (input)
                {
                    case "r":
                    case "rock":
                        pick = 0;
                        break;
                    case "p":
                    case "paper":
                        pick = 1;
                        break;
                    case "scissors":
                    case "s":
                        pick = 2;
                        break;
                    default:
                        return;
                }

                var choice = new Random().Next(0, 3);

                string msg;
                if (pick == choice)
                    msg = "We both chose: " + GetRpsPick(pick) + " Draw, Try again";
                else if (pick == 0 && choice == 1 ||
                         pick == 1 && choice == 2 ||
                         pick == 2 && choice == 0)
                    msg = "My Pick: " + GetRpsPick(choice) + "Beats Your Pick: " + GetRpsPick(pick) +
                          "\nYou Lose! Try Again!";
                else
                    msg = "Your Pick: " + GetRpsPick(pick) + "Beats My Pick: " + GetRpsPick(choice) +
                          "\nCongratulations! You win!";


                var embed = new EmbedBuilder
                {
                    Title = ":game_die: PassiveBOT The Expert Gamer :game_die:",
                    Description = $"{msg}",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                };
                await ReplyAsync("", false, embed.Build());
            }
        }

        private static string GetRpsPick(int p)
        {
            switch (p)
            {
                case 0:
                    return ":waning_gibbous_moon: ";
                case 1:
                    return ":newspaper:";
                default:
                    return "✂️";
            }
        }

        [Command("tableflip")]
        [Summary("tableflip")]
        [Alias("flip")]
        [Remarks("Flips a table")]
        public async Task Flip()
        {
            await ReplyAsync("(╯°□°）╯︵ ┻━┻");
        }

        [Command("dice")]
        [Summary("dice")]
        [Alias("roll")]
        [Remarks("roll a dice")]
        public async Task Dice()
        {
            var rnd = new Random();
            var embed = new EmbedBuilder
            {
                Title = ":game_die: PassiveBOT Rolled the Dice :game_die:",
                ImageUrl = $"{FunStr.Dice[rnd.Next(0, FunStr.Dice.Length)]}"
            };

            await ReplyAsync("", false, embed.Build());
        }

        [Command("coin")]
        [Summary("coin")]
        [Remarks("Flips a coin")]
        public async Task Coin()
        {
            var rand = new Random();
            var val = rand.Next(0, 100);
            string result;
            string coin;
            if (val >= 50)
            {
                coin =
                    "http://www.marshu.com/articles/images-website/articles/presidents-on-coins/quarter-coin-tail-thumb.jpg";
                result = "You Flipped **Tails!!**";
            }
            else
            {
                coin =
                    "http://www.marshu.com/articles/images-website/articles/presidents-on-coins/quarter-coin-head-thumb.jpg";
                result = "You Flipped **Heads!!**";
            }

            var embed = new EmbedBuilder
            {
                ImageUrl = coin,
                Description = result
            };
            await ReplyAsync("", false, embed.Build());
        }
    }
}