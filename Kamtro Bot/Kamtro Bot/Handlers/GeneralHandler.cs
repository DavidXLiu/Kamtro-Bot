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

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// Handler for all events that do not need their own class.
    /// </summary>
    public class GeneralHandler
    {
        public Dictionary<ulong, CrossBanDataNode> CrossBan;

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

        public void LoadList() {
            string json = FileManager.ReadFullFile(DataFileNames.AutoBanFile);

            CrossBan = JsonConvert.DeserializeObject<Dictionary<ulong, CrossBanDataNode>>(json);

            KLog.Info("Loaded AutoBan JSON into object");
        }

        private void SaveList() {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(DataFileNames.AutoBanFile))) {
                JsonSerializer js = new JsonSerializer();
                js.Formatting = Formatting.Indented;
                js.Serialize(sw, CrossBan);
            }

            KLog.Info("Saved autoban list.");
        }
    }
}
 