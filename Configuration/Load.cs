using PassiveBOT.Handlers;

namespace PassiveBOT.Configuration
{
    public class Load
    {
        public static string Gamesite = "PassiveNation.com";
        public static string Siteurl = "http://passivenation.com/";
        public static string Owner = "PassiveModding";
        public static string Version = "7.1";
        public static string Pre;
        public static int Messages;
        public static int Commands;
        public static string DBLLink = "https://discordbots.org/bot/303710071387324416";

        public static string Invite = $"https://discordapp.com/oauth2/authorize?client_id={Program.Client.CurrentUser.Id}&scope=bot&permissions=2146958591";


        public static string Server =  Config.Load().SupportServer;
    }
}