using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Fun : ModuleBase
    {
        [Command("Ping"), Summary("ping"), Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            string descrption =
             $"**Server Speed:** { Gateway} ms\n" +
             $"Wow, super fast!\n" +
             $"PassiveBOT is amazing isnt it?";
            var embed = new EmbedBuilder
            {
                Title = "🏓 PassiveBOT 🏓",
                Description = descrption,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
            };
            await ReplyAsync("", false, embed.Build());

        }

        [Command("8ball"), Summary("8ball 'am I special?'"), Remarks("ask me anything")]
        public async Task Ball([Remainder, Optional] string input)
        {
            if (input == null)
            {
                await ReplyAsync($"Ask me a question silly, eg. `.8ball am I special?`");
            }
            else
            {
                int result;
                Random rnd = new Random();
                result = rnd.Next(0, Strings.answers.Length);

                var embed = new EmbedBuilder
                {
                    Title = ":crystal_ball: PassiveBOT the bot that knows all :crystal_ball:",
                    Description = $"❓ {input}\n 🎱 {Strings.answers[result]}",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                };

                await ReplyAsync("", false, embed.Build());
            }

        }

        [Command("fortune"), Summary("fortune"), Remarks("open a fortune cookie")]
        public async Task Fortune()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings.answers.Length);

            var embed = new EmbedBuilder
            {
                Title = ":crystal_ball: PassiveBOT the Gypsy :crystal_ball:",
                Description = $"{Strings.fortune[result]}",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            await ReplyAsync("", false, embed.Build());

        }

        [Command("rps"), Summary("rps 'r'"), Remarks("rock paper scissors!")]
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
                Func<int, string> getRpsPick = (p) => {
                    switch (p)
                    {
                        case 0:
                            return ":waning_gibbous_moon: ";
                        case 1:
                            return ":newspaper:";
                        default:
                            return "✂️";
                    }
                };

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
                    msg = "We both chose: " + getRpsPick(pick) + " Draw, Try again";
                else if ((pick == 0 && choice == 1) ||
                 (pick == 1 && choice == 2) ||
                 (pick == 2 && choice == 0))
                    msg = "My Pick: " + getRpsPick(choice) + "Beats Your Pick: " + getRpsPick(pick) + "\nYou Lose! Try Again!";
                else
                    msg = "Your Pick: " + getRpsPick(pick) + "Beats My Pick: " + getRpsPick(choice) + "\nCongratulations! You win!";


                var embed = new EmbedBuilder
                {
                    Title = ":game_die: PassiveBOT The Expert Gamer :game_die:",
                    Description = $"{msg}",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
                };
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("tableflip"), Summary("tableflip"), Alias("flip"), Remarks("Flips a table")]
        public async Task Flip()
            =>await ReplyAsync("(╯°□°）╯︵ ┻━┻");

        [Command("dice"), Summary("dice"), Alias("roll"), Remarks("roll a dice")]
        public async Task Dice()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings.dice.Length);

            var embed = new EmbedBuilder
            {
                Title = ":game_die: PassiveBOT Rolled the Dice :game_die:",
                ImageUrl = $"{Strings.dice[result]}"
            };

            await ReplyAsync("", false, embed.Build());
        }

        [Command("coin"), Summary("coin"), Remarks("Flips a coin")]
        public async Task Coin()
        {
            Random rand = new Random();
            int val = (rand.Next(0, 100));
            var coin = "";
            var result = "";
            if (val >= 50)
            {
                coin = "http://www.marshu.com/articles/images-website/articles/presidents-on-coins/quarter-coin-tail-thumb.jpg";
                result = "You Flipped **Tails!!**";
            }
            else
            {
                coin = "http://www.marshu.com/articles/images-website/articles/presidents-on-coins/quarter-coin-head-thumb.jpg";
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