using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace PassiveBOT.Handlers
{
    //Replaced loghandler with colourlog cause thats sexy!
    public class ColourLog
    {
        public static Task ColourInfo(string message)
        {
            message = message.Replace("\n", " ");
            Console.WriteLine($"{DateTime.Now} [Info]  {message}", Color.Aqua);
            return Task.CompletedTask;
        }

        public static Task ColourError(string message)
        {
            message = message.Replace("\n", " ");
            Console.WriteLine($"{DateTime.Now} [Error] {message}", Color.Red);
            return Task.CompletedTask;
        }

        public static Task ColourDebug(string message)
        {
            message = message.Replace("\n", " ");
            var msg = message.Substring(21, message.Length - 21);
            Console.WriteLine($"{DateTime.Now} [Debug] {msg}", Color.GreenYellow);
            return Task.CompletedTask;
        }
    }
}