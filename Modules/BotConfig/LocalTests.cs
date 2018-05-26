using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using PassiveBOT.Discord;
using PassiveBOT.Discord.Context;
using PassiveBOT.Handlers;
using Sparrow.Platform.Posix.macOS;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    public class LocalTests : Base
    {
        [Command("Welcome_Event")]
        [Summary("Welcome_Event <@USER>")]
        [Remarks("Trigger a welcome event in the current server")]
        public async Task WelcomeEvent(SocketGuildUser User)
        {
            await Discord.Extensions.EventTriggers._client_UserJoined(User);
        }

        [Command("Goodbye_Event")]
        [Summary("Goodbye_Event <@USER>")]
        [Remarks("Trigger a Goodbye event in the current server")]
        public async Task GoodbyeEvent(SocketGuildUser User)
        {
            await Discord.Extensions.EventTriggers._client_UserLeft(User);
        }

        [Command("Get_Data")]
        [Summary("Get_Data")]
        [Remarks("Get the bot saved config for the current server")]
        public async Task GetData()
        {
            var DC = DatabaseHandler.GetGuild(Context.Guild.Id);
            var serialised = JsonConvert.SerializeObject(DC, Formatting.Indented);

            var uniEncoding = new UnicodeEncoding();   
            using (Stream ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, uniEncoding);
                try
                {
                    sw.Write(serialised);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                    await Context.Channel.SendFileAsync(ms, $"{Context.Guild.Name}[{Context.Guild.Id}] BotConfig.json");
                }
                finally
                {
                    sw.Dispose();
                }
            }
        }


        private readonly TimerService _service;
        public LocalTests(TimerService service)
        {
            _service = service;
        }

        [Command("Partner_Restart")]
        [Summary("Partner_Restart")]
        [Remarks("Restart the partner service")]
        public async Task Partner_Restart()
        {
            _service.Restart();
            await ReplyAsync("Timer (re)started.");
        }

        [Command("Partner_Trigger", RunMode = RunMode.Async)]
        [Summary("Partner_Trigger")]
        [Remarks("Trigger the partner service")]
        public async Task Partner_Trigger()
        {
            await _service.Partner();
        }

        [Command("Partner_Stop")]
        [Summary("Partner_Stop")]
        [Remarks("Stop the partner service")]
        public async Task Partner_Stop()
        {
            _service.Stop();
            await ReplyAsync("Timer stopped.");
        }
    }
}
