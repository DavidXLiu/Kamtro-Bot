﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Discord;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Newtonsoft.Json;
using System.Threading;

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
        public static Thread Autosave;

        private DiscordSocketClient client;
        private DiscordSocketConfig config;

        private CommandHandler _commands;
        private LogHandler _logs;

        public static FileManager fileManager;
        public static UserDataManager userDataManager;

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

            Autosave = new Thread(new ThreadStart(BotUtils.AutoSave));  // Create the thread. This will be started in StartAsync.

            new Program().StartAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync() {
            config = new DiscordSocketConfig() { MessageCacheSize = 1000000 }; // initialize the config for the client, and set the message cache size
            client = new DiscordSocketClient(config); // get the client with the configurations we want

            // Initialize 
            SetupFiles();  // This is to keep the StartAsync method more readable

            // Managers
            userDataManager = new UserDataManager(); // This sets up the user data files and loads them into memory
            fileManager = new FileManager();  // initialize the file manager

            // Initialize Handlers
            _commands = new CommandHandler(client);
            _logs = new LogHandler();

            BotUtils.SaveReady = true; // tell the class that the autosave loop should start
            Autosave.Start();  // Start the autosave loop

            await client.LoginAsync(TokenType.Bot, GetToken());
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private void SetupFiles() {
            // Check for the appropriate directories.
            foreach(string dir in DataFileNames.Folders) {  // Check through all the necessary directories
                if(!Directory.Exists(dir)) {  // If the directory does not exist
                    Directory.CreateDirectory(dir);  // Then create it
                } 
            }

            // We need a special case for the config
            if(!File.Exists(DataFileNames.GeneralConfigFile)) {  // If there isn't a config
                Settings = new BotSettings("!");  // Create a default one
                Settings.SaveJson();  // Save it
            }

            // Now for the files
            // This loop uses Reflection to iterate through all of the file paths and create any missing files
            // The settings.json file is the only one that needs a default template generated for it, and was handled above.
            foreach (FieldInfo fieldInfo in typeof(DataFileNames).GetFields(BindingFlags.Static | BindingFlags.Public)) {
                string file = fieldInfo.GetValue(null) as string;
                Console.WriteLine($"Generated {file}");
                File.CreateText(file).Close();  // This creates the file, then closes the unecessary stream writer
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
