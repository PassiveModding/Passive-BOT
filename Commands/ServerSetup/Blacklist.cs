using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    [Group("Blacklist")]
    public class Blacklist : InteractiveBase
    {
        [Command]
        [Summary("blacklist")]
        [Remarks("displays the blacklist")]
        public async Task B()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            var pages = new List<PaginatedMessage.Page>();
            var sb = new StringBuilder();
            foreach (var blacklistw in jsonObj.Antispams.Blacklist.BlacklistWordSet)
            {
                if (sb.ToString().Length >= 800)
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        description = sb.ToString()
                    });
                    sb.Clear();
                }

                sb.Append($"**Word(s)**\n" +
                          $"{string.Join("\n", blacklistw.WordList)}\n" +
                          $"**Response**\n" +
                          $"{blacklistw?.BlacklistResponse ?? jsonObj.Antispams.Blacklist.DefaultBlacklistMessage}\n\n");
            }

            pages.Add(new PaginatedMessage.Page
            {
                description = sb.ToString()
            });
            var pager = new PaginatedMessage
            {
                Title = "Blacklisted Messages :stop_button: to remove",
                Pages = pages
            };

            await PagedReplyAsync(pager, true, true, true);
        }

        [Command("formathelp")]
        [Summary("blacklist formathelp")]
        [Remarks("help with adding multiple phrases or words to the blacklist at once")]
        public async Task FormatHelp()
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Description = $"__**Sentences and Multiple Words**__\n" +
                              $"To add a sentence to the blacklist, use the add command like so:\n" +
                              $"`{Config.Load().Prefix}blacklist add this_is_a_sentence <response>`" +
                              $"You may leave the response empty to use the default blacklist message as your response\n" +
                              $"To add multiple words at once to the blacklist separate words using commas like so:\n" +
                              $"`{Config.Load().Prefix}blacklist add word1,word2,sentence_1,word3,sentence2 <response>`\n" +
                              $"Note that this will also work for the blacklist delete command\n\n" +
                              $"**__Responses__**\n" +
                              $"You can add custom text into the response by using the following custom tags:\n" +
                              "{user} - the user's username\n" +
                              "{user.mention} - @ the user\n" +
                              "{guild} - the guild's name\n" +
                              "{channel} - the current channel name\n" +
                              "{channel.mention} - #channel mention"
            });
        }

        [Command("add")]
        [Summary("blacklist add <word> <response>")]
        [Remarks(
            "adds a word to the blacklist, leave response blank to use the default message, use the same response for different blacklisted words to be grouped. Also separate sentences like so: hi_there_person for the keyword")]
        public async Task Ab(string keyword, [Remainder] string response = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            keyword = keyword.Replace("_", " ");
            var keywords = keyword.Split(',').Select(x => x.ToLower()).ToList();
            if (!jsonObj.Antispams.Blacklist.BlacklistWordSet.Any(x => x.WordList.Contains(keyword)))
            {
                var blacklistunit =
                    jsonObj.Antispams.Blacklist.BlacklistWordSet.FirstOrDefault(
                        x => x.BlacklistResponse == response);
                if (blacklistunit != null)
                {
                    blacklistunit.WordList.AddRange(keywords);
                    await Context.Message.DeleteAsync();
                    await ReplyAsync("Added to the Blacklist");
                }
                else
                {
                    blacklistunit = new GuildConfig.antispams.blacklist.BlacklistWords
                    {
                        WordList = keywords,
                        BlacklistResponse = response
                    };
                    jsonObj.Antispams.Blacklist.BlacklistWordSet.Add(blacklistunit);
                    await Context.Message.DeleteAsync();
                    await ReplyAsync("Added to the Blacklist");
                }
            }
            else
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync("Keyword is already in the blacklist");
                return;
            }

            GuildConfig.SaveServer(jsonObj);
        }

        [Command("del")]
        [Summary("blacklist del <word>")]
        [Remarks("removes a word from the blacklist")]
        public async Task Db(string initkeyword)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            initkeyword = initkeyword.Replace("_", " ");
            var keywords = initkeyword.Split(',').Select(x => x.ToLower()).ToList();
            foreach (var keyword in keywords)
            {
                var blacklistunit =
                    jsonObj.Antispams.Blacklist.BlacklistWordSet.FirstOrDefault(x =>
                        x.WordList.Contains(keyword.ToLower()));
                if (blacklistunit != null)
                {
                    blacklistunit.WordList.Remove(keyword.ToLower());
                    if (blacklistunit.WordList.Count == 0)
                        jsonObj.Antispams.Blacklist.BlacklistWordSet.Remove(blacklistunit);
                    await ReplyAsync($"{keyword} is has been removed from the blacklist");
                }
                else
                {
                    await ReplyAsync($"{keyword} is not in the blacklist");
                }
            }

            GuildConfig.SaveServer(jsonObj);
        }

        [Command("clear")]
        [Summary("blacklist clear")]
        [Remarks("clears the blacklist")]
        public async Task Clear()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Blacklist.BlacklistWordSet =
                new List<GuildConfig.antispams.blacklist.BlacklistWords>();
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist has been cleared.");
        }

        /*
        [Command("BFToggle")]
        [Summary("blacklist BFToggle")]
        [Remarks("Toggles whether or not to filter special characters for spam")]
        public async Task BFToggle()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Blacklist.BlacklistBetterFilter = !jsonObj.Antispams.Blacklist.BlacklistBetterFilter;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync(
                $"Blacklist BetterFilter status set to {(jsonObj.Antispams.Blacklist.BlacklistBetterFilter ? "ON" : "OFF")}");
        }
        */
        [Command("defaultmessage")]
        [Summary("blacklist defaultmessage <message>")]
        [Remarks("set the default blacklist message")]
        public async Task BlMessage([Remainder] string blmess = "")
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Blacklist.DefaultBlacklistMessage = blmess;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The default blacklist message is now:\n" +
                             $"{jsonObj.Antispams.Blacklist.DefaultBlacklistMessage}");
        }
    }
}