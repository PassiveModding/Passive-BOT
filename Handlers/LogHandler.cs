using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace PassiveBOT.Handlers
{
    //Replaced loghandler with colourlog cause thats sexy!
    public class ColourLog
    {
        public static Task In3(string command, char type, string server, char res, string user, Color colour)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20
            server = $"{server}                          ".Substring(0, 20); //trim param2 to 15

            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Info]  {command} | {type}: {server} | {res}: {user}", colour);
            return Task.CompletedTask;
        }

        public static Task In3Error(string command, char type, string server, char res, string user)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20
            server = $"{server}                          ".Substring(0, 20); //trim param2 to 15

            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Error] {command} | {type}: {server} | {res}: {user}", Color.Red);
            return Task.CompletedTask;
        }

        public static Task In2(string command, char type, string server, Color colour)
        {
            command = $"{command}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20

            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Info]  {command} | {type}: {server}", colour);
            return Task.CompletedTask;
        }

        public static Task In2Error(string one, char type, string error)
        {
            one = $"{one}                         ".Substring(0, 20).Replace("\n", " "); //trim param 1 to 20

            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Error] {one} | {type}: {error}", Color.Red);
            return Task.CompletedTask;
        }

        public static Task In1Run(string one)
        {
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Run]   {one}", Color.Gold);
            return Task.CompletedTask;
        }

        public static Task Debug(string message)
        {
            message = message.Replace("\n", " ");
            var msg = message.Substring(21, message.Length - 21);

            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy hh:mm:ss tt} [Debug] PassiveBOT           | {msg}", Color.GreenYellow);
            return Task.CompletedTask;
        }
    }
}