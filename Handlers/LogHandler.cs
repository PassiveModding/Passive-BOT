using Discord;
using PassiveBOT.Discord.Context;
using Serilog;

namespace PassiveBOT.Handlers
{
    public static class LogHandler
    {
        public static string Left(this string s, int len)
        {
            return s.Length == len ? s : (s.Length < len ? s.PadRight(len) : s.Substring(0, len));
        }

        public static void LogMessage(Context Context, string message = null, LogSeverity Level = LogSeverity.Info)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var custom = $"G: {Context.Guild.Name.Left(20)} || C: {Context.Channel.Name.Left(20)} || U: {Context.User.Username.Left(20)} || M: {Context.Message.Content.Left(100)}";

            if (message != null)
            {
                custom += $"\nE: {message.Left(100)}";
            }

            switch (Level)
            {
                case LogSeverity.Info:
                    Log.Information(custom);
                    break;
                case LogSeverity.Warning:
                    Log.Warning(custom);
                    break;
                case LogSeverity.Error:
                    Log.Error(custom);
                    break;
                case LogSeverity.Debug:
                    Log.Debug(custom);
                    break;
                case LogSeverity.Critical:
                    Log.Fatal(custom);
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(custom);
                    break;
                default:
                    Log.Information(message);
                    break;
            }
        }


        public static void LogMessage(string message, LogSeverity Level = LogSeverity.Info)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            switch (Level)
            {
                case LogSeverity.Info:
                    Log.Information(message);
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message);
                    break;
                case LogSeverity.Error:
                    Log.Error(message);
                    break;
                case LogSeverity.Debug:
                    Log.Debug(message);
                    break;
                case LogSeverity.Critical:
                    Log.Fatal(message);
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message);
                    break;
                default:
                    Log.Information(message);
                    break;
            }
        }
    }
}