using System;
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
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot
{
    /// <summary>
    /// This is the entry point to the program, and the startup class for the bot.
    /// </summary>
    /// <remarks>
    /// Completed I think.
    /// -C
    /// </remarks>
    public class Program
    {
        public const string Version = "0.5.3";
        private const string TokenFile = "token.txt";

        public static bool Ready = false;

        public static BotSettings Settings;

        public static Thread Autosave;
        public static Thread GarbageCollection;
        public static Thread DateCheck;
        public static Thread AutobanReset;

        public static DiscordSocketClient Client;
        private DiscordSocketConfig config;

        private CommandHandler _commands;
        private LogHandler _logs;
        private ReactionHandler _reaction;
        private GeneralHandler _general;

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
            Console.WriteLine("╚════════════════╝");
            Console.WriteLine("\n------------------\n");

            Autosave = new Thread(new ThreadStart(BotUtils.AutoSave));  // Create the thread. This will be started in StartAsync.
            GarbageCollection = new Thread(new ThreadStart(BotUtils.GarbageCollection));
            DateCheck = new Thread(new ThreadStart(BotUtils.DateEventCheckLoop));
            AutobanReset = new Thread(new ThreadStart(GeneralHandler.ResetThread));

            SetupFiles();  // This is to keep the Main method more readable

            new Program().StartAsync().GetAwaiter().GetResult();
        }


        public async Task StartAsync() {
            config = new DiscordSocketConfig() { MessageCacheSize = 1000000 }; // initialize the config for the client, and set the message cache size
            Client = new DiscordSocketClient(config); // get the client with the configurations we want

            // Initialize 

            // Managers
            userDataManager = new UserDataManager(); // This sets up the user data files and loads them into memory
            fileManager = new FileManager();  // initialize the file manager

            // Initialize Handlers
            _commands = new CommandHandler(Client);
            _logs = new LogHandler();
            _reaction = new ReactionHandler(Client);
            _general = new GeneralHandler(Client);

            Client.Ready += OnReady;  // Add the OnReady event

            BotUtils.SaveReady = true; // Tell the class that the autosave loop should start
            BotUtils.GCReady = true;
            Autosave.Start();  // Start the autosave loop
            GarbageCollection.Start();
            DateCheck.Start();
            AutobanReset.Start();

            await Client.LoginAsync(TokenType.Bot, GetToken());
            await Client.StartAsync();

            KLog.Important("Logged in!");

            await Task.Delay(-1);  // Stop this method from exiting.
        }

        public async Task OnReady() {
            SetupGeneral();
            Ready = true;
            KLog.Important("Ready!");
        }

        public static void LoadSettings() {
            // We need a special case for the config
            if (!File.Exists(DataFileNames.GeneralConfigFile)) {  // If there isn't a config
                File.CreateText(DataFileNames.GeneralConfigFile).Close();
                Settings = new BotSettings("!");  // Create a default one
                Settings.SaveJson();  // Save it
            } else {
                Settings = JsonConvert.DeserializeObject<BotSettings>(FileManager.ReadFullFile(DataFileNames.GeneralConfigFile));  // Load from the file
            }
        }

        public static void ReloadConfig() {
            LoadSettings();
            ServerData.SetupServerData(Settings);
            KLog.Info("Settings Reloaded");
        }
        public static void SaveSettings() {
            BotUtils.WriteToJson(Settings, DataFileNames.GeneralConfigFile);
            KLog.Info("Settings Saved.");
        }

        private static void SetupFiles() {
            // Check for the appropriate directories.
            foreach(string dir in DataFileNames.Folders) {  // Check through all the necessary directories
                if(!Directory.Exists(dir)) {  // If the directory does not exist
                    Directory.CreateDirectory(dir);  // Then create it
                } 
            }

            LoadSettings();

            // Special case for Excel files.
            // Only used for admin stuff atm
            AdminDataManager.InitExcel();

            // Now for the files
            // This loop uses Reflection to iterate through all of the file paths and create any missing files
            // The settings.json file is the only one that needs a default template generated for it, and was handled above.
            // The passedFolders variable is so that it skips the folder array.
            bool passedFolders = false;
            string file;

            foreach (FieldInfo fieldInfo in typeof(DataFileNames).GetFields(BindingFlags.Static | BindingFlags.Public)) {
                if(!passedFolders) {
                    passedFolders = true;
                    continue;
                }

                file = fieldInfo.GetValue(null) as string;

                if(!File.Exists(file)) {
                    Console.WriteLine($"Generated {file}");
                    File.CreateText(file).Close();  // This creates the file, then closes the unecessary stream writer
                }
            }

            long date;

            if (long.TryParse(FileManager.ReadFullFile(DataFileNames.LastDateFile), out date)) {
                BotUtils.LastWeeklyReset = new DateTime(date);
            }
        }

        private static void SetupGeneral() {
            ServerData.SetupServerData(Settings);
        }

        /// <summary>
        /// This method reads the token from the file named token.txt
        /// </summary>
        /// -C
        /// <returns>the token as a string</returns>
        private string GetToken() {
            if (!File.Exists("token.txt")) {
                // if there is no token file  -C
                Console.WriteLine("\nNo token.txt file found!\nPress any key to exit...");  // notify the user  -C
                Console.ReadKey();  // wait for the keypress  -C
                Environment.Exit(0);  // then exit  -C
            }

            // If the token file exists, then read it and return the token
            return FileManager.ReadFullFile("token.txt");
        }
    }
}
