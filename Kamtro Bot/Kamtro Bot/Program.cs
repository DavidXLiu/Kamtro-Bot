using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Newtonsoft.Json;

namespace Kamtro_Bot
{
    /// <summary>
    /// This is the entry point to the program, and the startup class for the bot.
    /// </summary>
    /// <remarks>
    /// Completed I think.
    /// -C
    /// </remarks>
    class Program
    {
        public const string Version = "0.0.1";  // We could manually change this, or do it differently. I'm going to leave it as it is for this test commit. -C
        private const string TokenFile = "token.txt";

        public static BotSettings Settings;
        private static string Token; // not currently initialized

        private DiscordSocketClient client;
        private DiscordSocketConfig config;

        private CommandHandler _commands;
        private LogHandler _logs;

        private FileManager fileManager;

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

            new Program().StartAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync() {
            config = new DiscordSocketConfig() { MessageCacheSize = 1000000 }; // initialize the config for the client, and set the message cache size
            client = new DiscordSocketClient(config); // get the client with the configurations we want

            // Initialize 
            fileManager = new FileManager();  // initialize the file manager
            SetupFiles();  // This is to keep the StartAsync method more readable

            // Initialize Handlers
            _commands = new CommandHandler();
            _logs = new LogHandler();

            await client.LoginAsync(TokenType.Bot, GetToken());
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private void SetupFiles() {
            try {
                Settings = BotSettings.LoadJson();  // load the config from the file  -C
            } catch(DirectoryNotFoundException e) {
                // the files aren't there, so create defaults. -C
                Console.WriteLine("There was no settings file!\nGenerating a default one...");  // Console message  -C

                Settings = new BotSettings("!"); // default prefix is ! -C
                File.CreateText(DataFileNames.CommandSettingsFile);  // Create the file  -C

                // I just copy-pasted this from the BotSettings.SaveJson method  -C
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(DataFileNames.CommandSettingsFile)) {
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        writer.Formatting = Formatting.Indented; // make it so that the entire file isn't on one line  -C
                        serializer.Serialize(writer, this);  // serialize the settings object and save it to the file  -C
                    }
                }
            }
        }

        /// <summary>
        /// This method reads the token from the file named token.txt
        /// -C
        /// </summary>
        /// <returns>the token as a string</returns>
        private string GetToken() {
            return fileManager.ReadFullFile("token.txt");
        }
    }
}
