using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kamtro_Bot.Interfaces.MessageEmbeds;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This module will contain some testing commands
    /// </summary>
    [Name("Testing")]
    public class TestingModule : ModuleBase<SocketCommandContext>
    {
        #region Ping Commands
        [Command("ping")]
        [Name("Ping")]
        [Summary("Simple ping command, bot responds with pong and latency.")]
        public async Task PingAsync() {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Pong! (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("pong")]
        [Name("Pong")]
        [Summary("Fun alternative to the ping command. Bot responds with ping and latency.")]
        public async Task PongAsync()
        {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Ping? (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("marco")]
        [Name("Marco")]
        [Summary("Another fun alternative to the ping command. Bot responds with ping and latency.")]
        public async Task MarcoAsync()
        {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Polo! (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("polo")]
        [Name("Polo")]
        [Summary("Fun and silly response in a user's expectations for a Polo command.")]
        public async Task PoloAsync()
        {
            await ReplyAsync(BotUtils.KamtroText($"But nobody came."));
        }
        #endregion
        #region Concept Commands
        
        [Command("messagetest")]
        [Alias("msgtst", "mts")]
        public async Task MessageTestAsync() {
            MessageTestEmbed mte = new MessageTestEmbed(Context);
            await mte.Display();
        }

        [Command("serverlist")]
        [RequireOwner]
        public async Task ServerListAsync() {
            foreach(SocketGuild server in Context.Client.Guilds) {
                Console.WriteLine($"[{server.Name}] Owned by: {BotUtils.GetFullUsername(server.Owner)}");
            }
        }

        [Command("throw")]
        public async Task ThrowAsync() {
            throw new Exception();
        }

        [Command("transfer")]
        public async Task TransferAsync()
        {
            BotUtils.PauseSave = true;
            string line = "";
            bool active = true;
            foreach (string file in Directory.GetFiles("OldUserFiles"))
            {
                // Loop through file once to check if user was ever active
                StreamReader sr = new StreamReader(file);
                while (!String.IsNullOrWhiteSpace(line = sr.ReadLine()))
                {
                    if (line.StartsWith("Activity Rating All Time:"))
                    {
                        if (line.Substring(line.IndexOf(':') + 1) != "0")
                        {
                            active = true;
                            break;
                        }
                        else
                        {
                            active = false;
                            break;
                        }
                    }
                }
                sr.Close();

                if (active)
                {
                    // Start reading again
                    sr = new StreamReader(file);

                    Console.WriteLine(file.Substring(file.LastIndexOf('\\') + 1, file.IndexOf('.') - (file.LastIndexOf('\\') + 1)));
                    ulong userId = ulong.Parse(file.Substring(file.LastIndexOf('\\') + 1, file.IndexOf('.') - (file.LastIndexOf('\\') + 1)));

                    line = sr.ReadLine();
                    Console.WriteLine(line);
                    string substr = line.Substring(line.IndexOf(':') + 2);
                    UserDataManager.AddUser(userId, line.Substring(line.IndexOf(':') + 2));
                    while (!String.IsNullOrWhiteSpace(line = sr.ReadLine()))
                    {
                        // Activity Rating
                        if (line.StartsWith("Activity Rating All Time:"))
                            UserDataManager.UserData[userId].Score = int.Parse(line.Substring(line.IndexOf(':') + 1));
                        // Rep
                        if (line.StartsWith("ReputationPoints:"))
                            UserDataManager.UserData[userId].Reputation = int.Parse(line.Substring(line.IndexOf(':') + 1));
                        // Profile Color
                        if (line.StartsWith("ProfileColor"))
                        {
                            // Parse string and get color
                            string[] strValues = line.Substring(line.IndexOf(':') + 1).Split(',');
                            int[] values = { int.Parse(strValues[0]), int.Parse(strValues[1]), int.Parse(strValues[2]) };
                            Color color = new Color(values[0], values[1], values[2]);

                            UserDataManager.UserData[userId].ProfileColor = color.RawValue;
                        }
                        // Quote
                        if (line.StartsWith("Logo"))
                            UserDataManager.UserData[userId].Quote = line.Substring(line.IndexOf(':') + 1);
                    }

                    // Porter
                    if (File.Exists("OldUserFiles/Exclusive/" + userId + ".txt"))
                        UserDataManager.UserData[userId].PorterSupporter = true;
                }
            }

            await ReplyAsync(BotUtils.KamtroText("Finished transfer!"));

            BotUtils.PauseSave = true;
        }
        #endregion
    }
}
