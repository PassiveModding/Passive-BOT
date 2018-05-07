using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.Configuration;
using PassiveBOT.Configuration.Objects;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands.Gaming
{
    public class GameStats : InteractiveBase
    {
        [Command("FortniteStats")]
        [Summary("FortniteStats <username>")]
        [Remarks("Get a fortnite user's profile")]
        [Ratelimit(1, 20, Measure.Seconds)]
        public async Task FortStats(string platform, [Remainder] string username)
        {
            if (Tokens.Load().FortniteToken == null)
            {
                await ReplyAsync(
                    "Fortnite Token has not been initialised for this bot, the owner must go to: https://fortnitetracker.com/site-api to get one");
                return;
            }

            if (platform.ToLower() != "pc" && platform.ToLower() != "xbl" && platform.ToLower() != "psn")
            {
                await FortStats();
                return;
            }

            var url = $"https://api.fortnitetracker.com/v1/profile/{platform.ToLower()}/{username}";
            var client = new WebClient();
            client.Headers.Add("TRN-Api-Key", Tokens.Load().FortniteToken);
            var stream = client.OpenRead(new Uri(url));

            var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var fstats = JsonConvert.DeserializeObject<FortniteProfile>(reader.ReadToEnd());
            var CurrentSoloSeasonStats = fstats.stats.curr_p2;
            var LifetimeSoloStats = fstats.stats.p2;

            var CurrentDuoStats = fstats.stats.curr_p10;
            var LifetimeDuoStats = fstats.stats.p10;

            var CurrentSquadStats = fstats.stats.curr_p9;
            var LifetimeSquadStats = fstats.stats.p9;

            var CurrentSolo = $"__**Current Solo Stats**__\n" +
                              $"KD: {CurrentSoloSeasonStats.kd.value}\n" +
                              $"Kills: {CurrentSoloSeasonStats.kills.value}\n" +
                              $"Kills/Game: {CurrentSoloSeasonStats.kpg.value}\n" +
                              $"Matches: {CurrentSoloSeasonStats.matches.value}\n" +
                              $"Score: {CurrentSoloSeasonStats.score.value}\n" +
                              $"Score/Game: {CurrentSoloSeasonStats.scorePerMatch.value}\n" +
                              $"Wins: {CurrentSoloSeasonStats.top1.value}\n" +
                              $"Top10: {CurrentSoloSeasonStats.top10.value}\n" +
                              $"Top25: {CurrentSoloSeasonStats.top25.value}\n" +
                              $"TRNRating: {CurrentSoloSeasonStats.trnRating.value}";
            var LifeSolo = $"__**Lifetime Solo Stats**__\n" +
                           $"KD: {LifetimeSoloStats.kd.value}\n" +
                           $"Kills: {LifetimeSoloStats.kills.value}\n" +
                           $"Kills/Game: {LifetimeSoloStats.kpg.value}\n" +
                           $"Matches: {LifetimeSoloStats.matches.value}\n" +
                           $"Score: {LifetimeSoloStats.score.value}\n" +
                           $"Score/Game: {LifetimeSoloStats.scorePerMatch.value}\n" +
                           $"Wins: {LifetimeSoloStats.top1.value}\n" +
                           $"Top10: {LifetimeSoloStats.top10.value}\n" +
                           $"Top25: {LifetimeSoloStats.top25.value}\n" +
                           $"TRNRating: {LifetimeSoloStats.trnRating.value}";
            var CurrentDuo = $"__**Current Duo Stats**__\n" +
                             $"KD: {CurrentDuoStats.kd.value}\n" +
                             $"Kills: {CurrentDuoStats.kills.value}\n" +
                             $"Kills/Game: {CurrentDuoStats.kpg.value}\n" +
                             $"Matches: {CurrentDuoStats.matches.value}\n" +
                             $"Score: {CurrentDuoStats.score.value}\n" +
                             $"Score/Game: {CurrentDuoStats.scorePerMatch.value}\n" +
                             $"Wins: {CurrentDuoStats.top1.value}\n" +
                             $"Top5: {CurrentDuoStats.top5.value}\n" +
                             $"Top12: {CurrentDuoStats.top12.value}\n" +
                             $"TRNRating: {CurrentDuoStats.trnRating.value}";
            var LifetimeDuo = $"__**Lifetime Duo Stats**__\n" +
                              $"KD: {LifetimeDuoStats.kd.value}\n" +
                              $"Kills: {LifetimeDuoStats.kills.value}\n" +
                              $"Kills/Game: {LifetimeDuoStats.kpg.value}\n" +
                              $"Matches: {LifetimeDuoStats.matches.value}\n" +
                              $"Score: {LifetimeDuoStats.score.value}\n" +
                              $"Score/Game: {LifetimeDuoStats.scorePerMatch.value}\n" +
                              $"Wins: {LifetimeDuoStats.top1.value}\n" +
                              $"Top5: {LifetimeDuoStats.top5.value}\n" +
                              $"Top12: {LifetimeDuoStats.top12.value}\n" +
                              $"TRNRating: {LifetimeDuoStats.trnRating.value}";

            var CurrentSquad = $"__**Current Squad Stats**__\n" +
                               $"KD: {CurrentSquadStats.kd.value}\n" +
                               $"Kills: {CurrentSquadStats.kills.value}\n" +
                               $"Kills/Game: {CurrentSquadStats.kpg.value}\n" +
                               $"Matches: {CurrentSquadStats.matches.value}\n" +
                               $"Score: {CurrentSquadStats.score.value}\n" +
                               $"Score/Game: {CurrentSquadStats.scorePerMatch.value}\n" +
                               $"Wins: {CurrentSquadStats.top1.value}\n" +
                               $"Top3: {CurrentSquadStats.top3.value}\n" +
                               $"Top6: {CurrentSquadStats.top6.value}\n" +
                               $"TRNRating: {CurrentSquadStats.trnRating.value}";
            var LifetimeSquad = $"__**Lifetime Squad Stats**__\n" +
                                $"KD: {LifetimeSquadStats.kd.value}\n" +
                                $"Kills: {LifetimeSquadStats.kills.value}\n" +
                                $"Kills/Game: {LifetimeSquadStats.kpg.value}\n" +
                                $"Matches: {LifetimeSquadStats.matches.value}\n" +
                                $"Score: {LifetimeSquadStats.score.value}\n" +
                                $"Score/Game: {LifetimeSquadStats.scorePerMatch.value}\n" +
                                $"Wins: {LifetimeSquadStats.top1.value}\n" +
                                $"Top3: {LifetimeSquadStats.top3.value}\n" +
                                $"Top6: {LifetimeSquadStats.top6.value}\n" +
                                $"TRNRating: {LifetimeSquadStats.trnRating.value}";

            var user = $"Fortnite Profile of: {fstats.epicUserHandle}\n" +
                       $"Platform: {fstats.platformName}\n";
            var recent = fstats.recentMatches.OrderByDescending(x => x.dateCollected).Select(x =>
                $"__**Recent Matches**__\n" +
                $"Time: {x.dateCollected}\n" +
                $"Match ID: {x.id}\n" +
                $"Kills: {x.kills}\n" +
                $"Matches: {x.matches}\n" +
                $"Minutes Played: {x.minutesPlayed}\n" +
                $"Playlist: {x.playlist}\n" +
                $"{(x.top1 == 0 ? "" : $"Win: {x.top1}\n")}" +
                $"{(x.top3 == 0 ? "" : $"Top3: {x.top3}\n")}" +
                $"{(x.top5 == 0 ? "" : $"Top5: {x.top5}\n")}" +
                $"{(x.top6 == 0 ? "" : $"Top6: {x.top6}\n")}" +
                $"{(x.top10 == 0 ? "" : $"Top10: {x.top10}\n")}" +
                $"{(x.top12 == 0 ? "" : $"Top12: {x.top12}\n")}" +
                $"{(x.top25 == 0 ? "" : $"Top25: {x.top25}\n")}" +
                $"Score: {x.score}").ToList();
            var pages = new List<string>
            {
                user,
                CurrentSolo,
                LifeSolo,
                CurrentDuo,
                LifetimeDuo,
                CurrentSquad,
                LifetimeSquad
            };
            var pgpage = pages.Select(x => new PaginatedMessage.Page
            {
                description = x
            });
            pages.AddRange(recent);
            var paginated = new PaginatedMessage
            {
                Pages = pgpage,
                Title = $"Fortnite Stats for: {fstats.epicUserHandle}",
                Color = Color.Blue
            };

            await PagedReplyAsync(paginated);
        }

        [Command("FortniteStats")]
        [Summary("FortniteStats <username>")]
        [Remarks("Get a fortnite user's profile")]
        [Ratelimit(1, 20, Measure.Seconds)]
        public async Task FortStats([Remainder] string username = null)
        {
            await ReplyAsync("Please choose a platform, \n" +
                             "`PC` - PC\n" +
                             "`PSN` - Playstation\n" +
                             "`XBL` - XBOX\n" +
                             "use the command like this:\n" +
                             $"`{Config.Load().Prefix}FortniteStats [Platform] [UserName]`\n" +
                             $"Eg.\n" +
                             $"`{Config.Load().Prefix}FortniteStats PC GamerGuy123`");
        }

        [Command("R6User")]
        [Summary("R6User <username>")]
        [Remarks("Get a r6s user profile")]
        public async Task R6User([Remainder] string username)
        {
            var stream = new WebClient().OpenRead($"https://api.r6stats.com/api/v1/players/{username}?platform=uplay");
            var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var user = JsonConvert.DeserializeObject<R6Profile.RootObject>(reader.ReadToEnd());

            var pages = new List<PaginatedMessage.Page>();
            var p = user.player;

            if (user.player.stats.ranked.has_played)
                pages.Add(new PaginatedMessage.Page
                {
                    description = $"**Ranked Stats**\n" +
                                  $"Kills: {p.stats.ranked.kills}\n" +
                                  $"Deaths: {p.stats.ranked.deaths}\n" +
                                  $"K/D: {p.stats.ranked.kd}\n" +
                                  $"Wins: {p.stats.ranked.wins}\n" +
                                  $"Losses: {p.stats.ranked.losses}\n" +
                                  $"W/L: {p.stats.ranked.wlr}\n" +
                                  $"Playtime (H): {TimeSpan.FromSeconds(p.stats.ranked.playtime).TotalHours}"
                });

            if (user.player.stats.casual.has_played)
                pages.Add(new PaginatedMessage.Page
                {
                    description = $"**Casual Stats**\n" +
                                  $"Kills: {p.stats.casual.kills}\n" +
                                  $"Deaths: {p.stats.casual.deaths}\n" +
                                  $"K/D: {p.stats.casual.kd}\n" +
                                  $"Wins: {p.stats.casual.wins}\n" +
                                  $"Losses: {p.stats.casual.losses}\n" +
                                  $"W/L: {p.stats.casual.wlr}\n" +
                                  $"Playtime (H): {TimeSpan.FromSeconds(p.stats.casual.playtime).TotalHours}"
                });


            pages.Add(new PaginatedMessage.Page
            {
                description = $"**Misc Stats**\n\n" +
                              $"Assists: {p.stats.overall.assists}\n" +
                              $"Barricades Built: {p.stats.overall.barricades_built}\n" +
                              $"Bullets Fired: {p.stats.overall.bullets_fired}\n" +
                              $"Bullets Hit: {p.stats.overall.bullets_hit}\n" +
                              $"Headshots: {p.stats.overall.headshots}\n" +
                              $"Melee Kills: {p.stats.overall.melee_kills}\n" +
                              $"Penetration Kills: {p.stats.overall.penetration_kills}\n" +
                              $"Revives: {p.stats.overall.revives}\n" +
                              $"Reinforcements Deployed: {p.stats.overall.reinforcements_deployed}\n" +
                              $"Steps Moved: {p.stats.overall.steps_moved}\n" +
                              $"Suicides: {p.stats.overall.suicides}"
            });

            var msg = new PaginatedMessage
            {
                Title = $"R6s Profile of {username}",
                Pages = pages,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }
    }
}