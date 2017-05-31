using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.preconditions;
using PassiveBOT.strings;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Fun : ModuleBase
    {
        [Command("Ping", RunMode = RunMode.Async)]
        [Alias("pong")]
        [Summary("ping")]
        [Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var gateway = client.Latency;
            var descrption =
                $"**Server Speed:** {gateway} ms\n" +
                "Wow, super fast!\n" +
                "PassiveBOT is amazing isnt it?";
            var embed = new EmbedBuilder
            {
                Title = "🏓 PassiveBOT 🏓",
                Description = descrption,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("8ball")]
        [Summary("8ball 'am I special?'")]
        [Remarks("ask me anything")]
        public async Task Ball([Remainder] [Optional] string input)
        {
            if (input == null)
            {
                await ReplyAsync("Ask me a question silly, eg. `.8ball am I special?`");
            }
            else
            {
                var rnd = new Random();
                var result = rnd.Next(0, FunStr.Answers.Length);

                var embed = new EmbedBuilder
                {
                    Title = ":crystal_ball: PassiveBOT the bot that knows all :crystal_ball:",
                    Description = $"❓ {input}\n 🎱 {FunStr.Answers[result]}",
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
            var result = rnd.Next(0, FunStr.Answers.Length);

            var embed = new EmbedBuilder
            {
                Title = ":crystal_ball: PassiveBOT the Gypsy :crystal_ball:",
                Description = $"{FunStr.Fortune[result]}",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("rps")]
        [Summary("rps 'r'")]
        [Remarks("rock paper scissors!")]
        public async Task Rps([Optional] string input)
        {
            if (input == null)
            {
                await ReplyAsync(
                    "❓ to play rock, paper, scissors" +
                    "n\n:waning_gibbous_moon: type `.rps rock` or `.rps r` to pick rock" +
                    "\n\n:newspaper: type `.rps paper` or `.rps p` to pick paper" +
                    "\n\n✂️ type `.rps scissors` or `.rps s` to pick scissors"
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
            var result = rnd.Next(0, FunStr.Dice.Length);

            var embed = new EmbedBuilder
            {
                Title = ":game_die: PassiveBOT Rolled the Dice :game_die:",
                ImageUrl = $"{FunStr.Dice[result]}"
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