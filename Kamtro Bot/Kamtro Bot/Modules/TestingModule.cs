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
using Kamtro_Bot.Util;
using Kamtro_Bot.Interfaces.BasicEmbeds;

using OfficeOpenXml;
using Kamtro_Bot.Interfaces.ActionEmbeds;

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
        #region Utility Commands
        [Command("time")]
        [Name("Time")]
        [Summary("A command that tells the current local time of the bot and the user.")]
        public async Task TimeAsync()
        {
            await ReplyAsync(BotUtils.KamtroText($"It is currently {DateTime.Now.ToLongTimeString()} in my timezone."));
        }
        #endregion
        #region Other Commands
        [Command("debug")]
        public async Task ToggleDebugAsync([Remainder] string args = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // permissions checking

            if (string.IsNullOrWhiteSpace(args)) {
                Program.Debug = !Program.Debug;
                await ReplyAsync(BotUtils.KamtroText($"Debug mode turned {(Program.Debug ? "on" : "off")}."));
                return;
            }

            if (args.ToLower() == "on") {
                Program.Debug = true;
                KLog.Important("Debug mode turned on");
                await ReplyAsync(BotUtils.KamtroText("Debug mode turned on."));
            } else if (args.ToLower() == "off") {
                Program.Debug = false;
                KLog.Important("Debug mode turned off");
                await ReplyAsync(BotUtils.KamtroText("Debug mode turned off."));
            } else if (args.ToLower() == "mode") {
                await ReplyAsync(BotUtils.KamtroText($"Debug mode is {(Program.Debug ? "on" : "off")}"));
            } else {
                await ReplyAsync(BotUtils.KamtroText("Invalid arguments. No args to toggle, '!debug on' to turn debug mode on, '!debug off' to turn debug mode off, '!debug mode' to see current debug mode."));
            }
        }
        
        [Command("credits")]
        [Alias("version", "v")]
        public async Task Credits([Remainder] string args = "") {
            // ignore the parameters
            CreditsEmbed ce = new CreditsEmbed();

            await ce.Display(Context.Channel);
        }
        
        [Command("changelog")]
        [Alias("cl")]
        public async Task ChangeLogAsync([Remainder] string args = "") {
            ChangeLogEmbed cle = new ChangeLogEmbed(Context);
            await cle.Display();
        }

        [Command("experimental")]
        [Alias("exp")]
        public async Task Experimental([Remainder] string args = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // permissions checking

            if (string.IsNullOrWhiteSpace(args)) {
                Program.Experimental = !Program.Experimental;
                await ReplyAsync(BotUtils.KamtroText($"Experimental mode turned {(Program.Experimental ? "on" : "off")}."));
                return;
            }

            if (args.ToLower() == "on") {
                Program.Experimental = true;
                KLog.Important("Experimental mode turned on");
                await ReplyAsync(BotUtils.KamtroText("Experimental mode turned on."));
            } else if (args.ToLower() == "off") {
                Program.Experimental = false;
                KLog.Important("Experimental mode turned off");
                await ReplyAsync(BotUtils.KamtroText("Experimental mode turned off."));
            } else if (args.ToLower() == "mode") {
                await ReplyAsync(BotUtils.KamtroText($"Experimental mode is {(Program.Experimental ? "on" : "off")}"));
            } else {
                await ReplyAsync(BotUtils.KamtroText("Invalid arguments. No args to toggle, '!Experimental on' to turn Experimental mode on, '!experimental off' to turn Experimental mode off, '!experimental mode' to see current Experimental mode."));
            }
        }
        
        [Command("convert")]
        public async Task ConvertAsync() {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;

            FileInfo f = new FileInfo("Strike_Log.xlsx");
            ExcelPackage p = new ExcelPackage(f);

            ExcelWorksheet log = p.Workbook.Worksheets["Sheet1"];

            int users = 0;
            int dates = 0;

            for(int i = 2; i <= 421; i++) {
                if (!string.IsNullOrWhiteSpace(log.Cells[$"E{i}"].Text)) {
                    log.Cells[$"E{i}"].Value = ConvertDT(log.Cells[$"E{i}"].Text, i).ToString("F"); dates++;
                }

                if (!string.IsNullOrWhiteSpace(log.Cells[$"G{i}"].Text)) {
                    log.Cells[$"G{i}"].Value = ConvertDT(log.Cells[$"G{i}"].Text, i).ToString("F"); dates++;
                }

                if (!string.IsNullOrWhiteSpace(log.Cells[$"I{i}"].Text)) {
                    log.Cells[$"I{i}"].Value = ConvertDT(log.Cells[$"I{i}"].Text, i).ToString("F"); dates++;
                }

                users++;
            }

            p.Save();

            await ReplyAsync(BotUtils.KamtroText($"Conveted {dates} dates for {users} users."));
        }

        [Command("debuglog")]
        [Alias("dlog", "printdebug")]
        public async Task DebugLogAsync([Remainder] string args = "") {
            int c = 40;

            if(!string.IsNullOrEmpty(args)) {
                if(!int.TryParse(args, out c)) {
                    await ReplyAsync(BotUtils.KamtroText("Please enter a valid number less than or eual to 40, or leave blank for 40."));
                }
            }

            string s = "";

            for(int i = 0; i < Math.Min(c, KLog.log.Count); i++) {
                s += $"```\n{KLog.log[i]}\n```";
            }

            await ReplyAsync(s);
        }

        private DateTime ConvertDT(string _d, int i) {
            KLog.Debug($"{i}");  // LOOK AT THIS, i Might be useless

            string[] dt = _d.Split(' ');

            string[] date = dt[0].Split('/');
            string[] time = dt[1].Split(':');

            int d, m, y, h, mi, s;

            // Date
            int.TryParse(date[0], out m);
            int.TryParse(date[1], out d);
            int.TryParse(date[2], out y);

            int.TryParse(time[0], out h);
            int.TryParse(time[1], out mi);
            int.TryParse(time[2], out s);

            h %= 12;  // Convert to 24 hour format part 1

            if (dt[2] == "PM") {
                h += 12;
            }

            DateTime ddd = new DateTime(y, m, d, h, mi, s);

            return ddd;
        }
        #endregion
        #region Concept Commands
        /*
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
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // permissions checking

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
        
        // */
        #endregion
    }
}
