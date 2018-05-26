using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Discord.Context;
using PassiveBOT.Handlers;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
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
    }
}
