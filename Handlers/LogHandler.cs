using System;
using System.Drawing;
using System.Threading.Tasks;
using Serilog;

namespace PassiveBOT.Handlers
{
    //Replaced loghandler with colourlog cause thats sexy!
    public class ColourLog
    {
        public static Task In3(string command, char type, string server, char res, string user, Color colour)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20
            server = $"{server}                          ".Substring(0, 20); //trim param2 to 15

            Console.WriteLine(
                $"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Info]  {command} | {type}: {server} | {res}: {user}", colour);
            return Task.CompletedTask;
        }

        public static Task In3Error(string command, char type, string server, char res, string user)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20
            server = $"{server}                          ".Substring(0, 20); //trim param2 to 15

            LogInfo(
                $"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Error] {command} | {type}: {server} | {res}: {user}");
            return Task.CompletedTask;
        }

        public static Task In2(string command, char type, string server, Color colour)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20

            LogInfo($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Info]  {command} | {type}: {server}");
            return Task.CompletedTask;
        }

        public static Task In2Error(string one, char type, string error)
        {
            one = $"{one}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20

            LogInfo($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Error] {one} | {type}: {error}");
            return Task.CompletedTask;
        }

        public static Task In1Run(string one)
        {
            LogInfo($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Run]   {one}");
            return Task.CompletedTask;
        }

        public static Task Debug(string message)
        {
            message = message.Replace("\n", " ");
            var msg = message.Substring(21, message.Length - 21);

           LogInfo($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Debug] PassiveBOT           | {msg}");
            return Task.CompletedTask;
        }

        public static void LogInfo(string message)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Information($"{message}");
        }

        public static void LogError(string message)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Error($"{message}");
        }
    }
}