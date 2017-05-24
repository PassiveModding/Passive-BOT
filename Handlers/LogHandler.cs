using System.Threading.Tasks;
using Serilog;

namespace PassiveBOT.Handlers
{
    public class LogHandler
    {
        public static Task LogAsync(string message)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Serilog.Log.Information($"{message}");
            return Task.CompletedTask;
        }

        public static Task LogErrorAsync(string message, string error)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Serilog.Log.Error($"      {message} | {error}");
            return Task.CompletedTask;
        }

        public static Task LogClientAsync(string message)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var msg = message.Substring(21, message.Length - 21); //removes unnecessary data added at the start of debug logging
            Serilog.Log.Debug($"{msg}");
            return Task.CompletedTask;
        }
    }
}
