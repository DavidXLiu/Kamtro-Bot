using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
{
    /// <summary>
    /// This is the entry point to the program, and the startup class for the bot.
    /// </summary>
    /// <remarks>
    /// Not done yet
    /// </remarks>
    class Program
    {
        public const string Version = "0.0.1";  // We could manually change this, or do it differently. I'm going to leave it as it is for this test commit. -C

        static void Main(string[] args)
        {
            Console.Title = $"Kamtro Bot v{Version}";  // Formatted string with $ before the "". Any text in the {} is treated as code. I mostly just use this for variables.

            Console.WriteLine("╔════════════════╗");
            Console.WriteLine("║   Kamtro Bot   ║");
            Console.WriteLine("╠════════════════╣");
            Console.WriteLine("║       By:      ║");
            Console.WriteLine("║      Arcy      ║");
            Console.WriteLine("║     Carbon     ║");
            Console.WriteLine("║     Lumina     ║");
            Console.WriteLine("╚════════════════╝");
            Console.WriteLine("\n------------------\n");
        }
    }
}
