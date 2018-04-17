using Discord;

namespace PassiveBOT.Configuration
{
    public class Load
    {
        public static string Gamesite = "PassiveNation.com";
        public static string Siteurl = "http://passivenation.com/";
        public static string Owner = "PassiveModding";
        public static string Version = "7.1";
        public static string Pre = Config.Load().Prefix;
        public static int Messages;
        public static int Commands;
        public static string GetInvite(IDiscordClient client)
        {
            return
                $"https://discordapp.com/oauth2/authorize?client_id={client.CurrentUser.Id}&scope=bot&permissions=2146958591";
        }
    }
}