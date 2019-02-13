namespace PassiveBOT.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Discord;

    using PassiveBOT.Context;
    using PassiveBOT.Models;

    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    /// <summary>
    ///     The Log handler.
    /// </summary>
    public static class LogHandler
    {
        /// <summary>
        ///     Gets or sets the Log.
        /// </summary>
        public static Logger Log { get; set; } = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();

        /// <summary>
        ///     Converts from discord LogSeverity to Serilog LogEventLevel
        /// </summary>
        /// <param name="level">The discord LogLevel</param>
        /// <returns>
        ///     The converted LogEventLevel
        /// </returns>
        public static LogEventLevel DiscordLogToEventLevel(LogSeverity level)
        {
            switch (level)
            {
                case LogSeverity.Info:
                    return LogEventLevel.Information;
                case LogSeverity.Warning:
                    return LogEventLevel.Warning;
                case LogSeverity.Error:
                    return LogEventLevel.Warning;
                case LogSeverity.Debug:
                    return LogEventLevel.Debug;
                case LogSeverity.Critical:
                    return LogEventLevel.Fatal;
                case LogSeverity.Verbose:
                    return LogEventLevel.Verbose;
                default:
                    return LogEventLevel.Information;
            }
        }

        /// <summary>
        ///     Ensures a string is aligned and kept to the specified length
        ///     Uses substring if it is too long and pads if too short.
        /// </summary>
        /// <param name="s">
        ///     The string to modify
        /// </param>
        /// <param name="len">
        ///     The desired string length
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public static string Left(this string s, int len)
        {
            return s.Length == len ? s : (s.Length < len ? s.PadRight(len) : s.Substring(0, len));
        }

        /// <summary>
        ///     Logs a message to console and RavenDB
        /// </summary>
        /// <param name="guildId">
        ///     The guild id.
        /// </param>
        /// <param name="channelName">
        ///     The channel name.
        /// </param>
        /// <param name="userId">
        ///     The user id.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="error">
        ///     The error if applicable
        /// </param>
        /// <param name="logSeverity">
        ///     The log severity.
        /// </param>
        public static void LogMessage(ulong guildId, string channelName, ulong userId, string message, string error = null, LogSeverity logSeverity = LogSeverity.Info)
        {
            var custom = $"G: {guildId.ToString().Left(20)} | C: {channelName.Left(20)} | U: {userId.ToString().Left(20)} | M: {message.Left(100)}";
            if (error != null)
            {
                custom += $"\nE: {error}";
            }

            LogMessage(custom, logSeverity);
        }

        /// <summary>
        ///     Logs a message to console with the specified severity. Includes info based on context
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="error">
        ///     Optional error message.
        /// </param>
        /// <param name="logSeverity">
        ///     The Severity of the message
        /// </param>
        public static void LogMessage(Context context, string error = null, LogSeverity logSeverity = LogSeverity.Info)
        {
            // var custom = $"G: {context.Guild.Name.Left(20)} || C: {context.Channel.Name.Left(20)} || U: {context.User.Username.Left(20)} || M: {context.Message.Content.Left(100)}";
            var custom = $"G: {context.Guild.Id.ToString().Left(20)} | C: {context.Channel.Name.Left(20)} | U: {context.User.Id.ToString().Left(20)} | M: {context.Message.Content.Left(100)}";

            if (error != null)
            {
                custom += $"\nE: {error}";
            }

            LogMessage(custom, logSeverity);
        }

        /// <summary>
        ///     Logs a message to console
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="logSeverity">
        ///     The severity of the message
        /// </param>
        public static void LogMessage(string message, LogSeverity logSeverity = LogSeverity.Info)
        {
            switch (logSeverity)
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

        /// <summary>
        ///     Prints application info to console
        /// </summary>
        /// <param name="settings">
        ///     The settings.
        /// </param>
        /// <param name="config">
        ///     The config.
        /// </param>
        public static void PrintApplicationInformation(DatabaseObject settings, ConfigModel config)
        {
            Console.WriteLine("-> INFORMATION\n" + $"-> Database URL(s): {string.Join("\n", settings?.Urls ?? new List<string>())}\n" + $"-> Database Name: {settings?.Name}\n" + $"-> Prefix: {config.Prefix}\n" + $"-> Shards: {config.Shards}\n" + $"    Author: PassiveModding | Discord: https://discord.me/Passive\n" + $"=======================[ {DateTime.UtcNow} ]=======================");
        }
    }
}