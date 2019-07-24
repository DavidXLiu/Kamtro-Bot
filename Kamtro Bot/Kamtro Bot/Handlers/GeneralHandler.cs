using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kamtro_Bot.Util;
using Kamtro_Bot.Nodes;
using Newtonsoft.Json;
using System.Threading;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// Handler for all events that do not need their own class.
    /// </summary>
    public class GeneralHandler
    {
        public static Dictionary<ulong, CrossBanDataNode> CrossBan;

        public static bool ResetOn = true;

        public static int ConsMessages = 0;
        public static ulong LastUser = 0;

        #region Event Handlers
        public GeneralHandler(DiscordSocketClient client) {
            // Setup
            LoadList();

            // Add the events
            client.GuildMemberUpdated += OnMemberUpdate;
            client.UserJoined += OnMemberJoin;
            client.UserUnbanned += OnMemberUnban;
        }

        /// <summary>
        /// Happens whenever a user joins a server the bot is in
        /// </summary>
        /// <param name="user">The user that joined</param>
        /// <returns></returns>
        public async Task OnMemberJoin(SocketGuildUser user) {
            // For cross ban.
            if(CrossBan.ContainsKey(user.Id)) {
                await BotUtils.AdminLog($"Cross-banned user {BotUtils.GetFullUsername(user)}. " + CrossBan[user.Id].GetInfoText());
                AdminDataManager.AddBan(user, new BanDataNode(Program.Client.CurrentUser, $"[X-ban | {CrossBan[user.Id].GetServer()}] {CrossBan[user.Id].Reason}"));
                await ServerData.Server.AddBanAsync(user);

                 KLog.Info($"Cross-banned user {BotUtils.GetFullUsername(user)}");
            }
        }

        /// <summary>
        /// Happens when a user is unbanned from a server the bot is in.
        /// </summary>
        /// <param name="user">The user that was unbanned</param>
        /// <param name="server">The server they were unbanned from</param>
        /// <returns></returns>
        public async Task OnMemberUnban(SocketUser user, SocketGuild server) {
            if(CrossBan.ContainsKey(user.Id) && server.Id == ServerData.Server.Id) {
                CrossBan.Remove(user.Id);
                KLog.Info($"Removed user {BotUtils.GetFullUsername(user)} from cross-ban list");
                SaveList();
            }
        }

        public async Task OnMemberUpdate(SocketGuildUser before, SocketGuildUser after) {
            if (before.Guild != ServerData.Server) return; // If it's not on kamtro, ignore it

            if(BotUtils.GetFullUsername(before) != BotUtils.GetFullUsername(after)) {
                // If the user changed their name.
            }

            if(before.Roles.Count != after.Roles.Count) {
                // role update
                foreach(SocketRole role in after.Roles) {
                    if(ServerData.SilencedRoles.Contains(role)) {
                        // remove mature role if user has it
                        if(after.Roles.Contains(ServerData.NSFWRole)) {
                            await after.RemoveRoleAsync(ServerData.NSFWRole);
                        }
                    }
                }
            }
        }
        #endregion

        #region Non-Handlers
        public static async Task HandleMessage(SocketUserMessage msg) {
            if (msg.Channel is ISocketPrivateChannel) return;

            SocketGuildUser auth = ServerData.Server.GetUser(msg.Author.Id);

            if (LastUser == 0 || auth.Id != LastUser) {
                LastUser = msg.Author.Id;
                ConsMessages = 1;
            } else if (ServerData.HasPermissionLevel(auth, ServerData.PermissionLevel.TRUSTED)) {
                ConsMessages = 0;
                LastUser = auth.Id;
            } else {
                ConsMessages++;
            }
            
            if(ConsMessages == 3) {
                await msg.Channel.SendMessageAsync(BotUtils.KamtroAngry($"Please slow down {BotUtils.GetFullUsername(msg.Author)}, or you will be autobanned for spam."));
            }

            if(ConsMessages >= 5) {
                await BotUtils.AdminLog($"User {BotUtils.GetFullUsername(auth)} was autobanned for spam");
                await ServerData.Server.AddBanAsync(auth);

                AdminDataManager.AddBan(msg.Author, new BanDataNode(Program.Client.CurrentUser, "Autobanned for spam"));

                KLog.Info($"Autobanned user {BotUtils.GetFullUsername(auth)} for spam.");
            }
        }
        #endregion

        #region Helper Methods
        public static void LoadList() {
            string json = FileManager.ReadFullFile(DataFileNames.AutoBanFile);

            CrossBan = JsonConvert.DeserializeObject<Dictionary<ulong, CrossBanDataNode>>(json);

            KLog.Info("Loaded AutoBan JSON into object");
        }

        public static void SaveList() {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(DataFileNames.AutoBanFile))) {
                JsonSerializer js = new JsonSerializer();
                js.Formatting = Formatting.Indented;
                js.Serialize(sw, CrossBan);
            }

            KLog.Info("Saved autoban list.");
        }
        #endregion

        #region Threads
        public static void ResetThread() {
            while(ResetOn) {
                ConsMessages = 0;
                Thread.Sleep(new TimeSpan(0, 0, 10));
            }
        }
        #endregion
    }
}
 