using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;

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
        private const string TokenFile = "token.txt";

        private static string Token; // not currently initialized

        private DiscordSocketClient client;
        private DiscordSocketConfig config;

        private CommandHandler _commands;
        private LogHandler _logs;

       

        static void Main(string[] args)
        {
            Console.Title = $"Kamtro Bot v{Version}";  // Formatted string with $ before the "". Any text in the {} is treated as code. I mostly just use this for variables. -C

            Console.WriteLine("╔════════════════╗");
            Console.WriteLine("║   Kamtro Bot   ║");
            Console.WriteLine("╠════════════════╣");
            Console.WriteLine("║       By:      ║");
            Console.WriteLine("║      Arcy      ║");
            Console.WriteLine("║     Carbon     ║");
            Console.WriteLine("║     Lumina     ║");
            Console.WriteLine("╚════════════════╝");
            Console.WriteLine("\n------------------\n");

            new Program().StarAsync().GetAwaiter().GetResult();
        }

        public async Task StarAsync() {
            config = new DiscordSocketConfig() { MessageCacheSize = 1000000 };
            client = new DiscordSocketClient(config);

            _commands = new CommandHandler();
            _logs = new LogHandler();

            await client.LoginAsync(TokenType.Bot, PrivateData.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
