using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Discord;

namespace PassiveBOT.Handlers
{
    public class LogHandler
    {
        public static Task LogAsync(string message)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information($"{message}");
            return Task.CompletedTask;
        }

        public static Task LogErrorAsync(string message, string error)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Error($"      {message} | {error}");
            return Task.CompletedTask;
        }

        public static Task LogClientAsync(string message)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var msg = message.Substring(21, message.Length - 21);
            Log.Debug($"      {msg}");
            return Task.CompletedTask;
        }
    }
}
