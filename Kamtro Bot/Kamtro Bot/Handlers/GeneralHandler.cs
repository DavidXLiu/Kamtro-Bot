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
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Discord;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// Handler for all events that do not need their own class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class GeneralHandler
    {
        public static Dictionary<ulong, CrossBanDataNode> CrossBan;

        public static bool ResetOn = true;

        public static Dictionary<ulong, int> ConsMessages;
        public static Dictionary<ulong, ulong> LastUser;

        #region Event Handlers
        public GeneralHandler(DiscordSocketClient client) {
            // Setup
            LoadList();

            // Add the events
            client.GuildMemberUpdated += OnMemberUpdate;
            client.UserJoined += OnMemberJoin;
            client.UserUnbanned += OnMemberUnban;
            client.MessageUpdated += OnMessageUpdate;
            client.MessageDeleted += OnMessageDelete;
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

        /// <summary>
        /// Happens whenever anything about a user (except roles) updates
        /// </summary>
        /// <remarks>
        /// This is pretty dumb. 
        /// It's up to you to figure out what changed. It could be anything from a new pfp or username,
        /// to changing your status from idle to online.
        /// </remarks>
        /// <param name="before">User before</param>
        /// <param name="after">User after</param>
        /// <returns></returns>
        public async Task OnMemberUpdate(SocketGuildUser before, SocketGuildUser after) {
            if (before.Guild != ServerData.Server) return; // If it's not on kamtro, ignore it

            if(BotUtils.GetFullUsername(before) != BotUtils.GetFullUsername(after)) {
                // If the user changed their name.
                NameChangeEmbed nce = new NameChangeEmbed(before, after);
                await nce.Display(ServerData.AdminChannel);
            }

            if (before.Nickname != after.Nickname) {
                // If the nickame changed
                NameChangeEmbed nce = new NameChangeEmbed(before, after, true);
                await nce.Display(ServerData.AdminChannel);
            }

            if(before.GetAvatarUrl() != after.GetAvatarUrl()) {
                AvatarUpdateEmbed emb = new AvatarUpdateEmbed(before, after);
                await emb.Display();
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
        
        /// <summary>
        /// Called whenever a user edits a message
        /// </summary>
        /// <param name="old">The old message</param>
        /// <param name="updated">The updated message</param>
        /// <returns></returns>
        public async Task OnMessageUpdate(Cacheable<IMessage, ulong> old, SocketMessage updated, ISocketMessageChannel channel) {
            IMessage msg = await old.GetOrDownloadAsync();

            if (msg.Source != MessageSource.User || !(msg is SocketMessage) || !(channel is SocketTextChannel)) return;

            SocketMessage sm = msg as SocketMessage;

            MessageEditEmbed messageEdit = new MessageEditEmbed(sm, updated);
            await messageEdit.Display(ServerData.AdminChannel);
        }
        
        /// <summary>
        /// Called when a user deletes a message
        /// </summary>
        /// <param name="message">The deleted message</param>
        /// <param name="channel">The channel it was deleted in</param>
        /// <returns></returns>
        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) {
            IMessage msg = await message.GetOrDownloadAsync();

            if (msg.Source != MessageSource.User || !(msg is SocketMessage) || !(channel is SocketTextChannel)) return;

            SocketMessage sm = msg as SocketMessage;

            MessageDeleteEmbed mde = new MessageDeleteEmbed(sm);
            await mde.Display(ServerData.AdminChannel);
        }
        #endregion

        #region Non-Handlers
        public static async Task HandleMessage(SocketUserMessage msg) {
            if (msg.Channel is ISocketPrivateChannel) return;

            if(!LastUser.ContainsKey(msg.Channel.Id)) {
                LastUser.Add(msg.Channel.Id, 0);
            }

            if(!ConsMessages.ContainsKey(msg.Channel.Id)) {
                ConsMessages.Add(msg.Channel.Id, 0);
            }

            SocketGuildUser auth = ServerData.Server.GetUser(msg.Author.Id);

            if (auth == null) return;

            if (LastUser[msg.Channel.Id] == 0 || auth.Id != LastUser[msg.Channel.Id]) {
                LastUser[msg.Channel.Id] = msg.Author.Id;
                ConsMessages[msg.Channel.Id] = 1;
            } else {
                ConsMessages[msg.Channel.Id]++;
            }
            
            if(ConsMessages[msg.Channel.Id] == 3 && !ServerData.HasPermissionLevel(auth, ServerData.PermissionLevel.TRUSTED)) {
                await msg.Channel.SendMessageAsync(BotUtils.KamtroAngry($"Please slow down {BotUtils.GetFullUsername(msg.Author)}, or you will be autobanned for spam."));
            }

            if(ConsMessages[msg.Channel.Id] >= 5 && !ServerData.HasPermissionLevel(auth, ServerData.PermissionLevel.TRUSTED)) {
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
                foreach(ulong key in ConsMessages.Keys) {
                    ConsMessages[key] = 0;
                }

                Thread.Sleep(new TimeSpan(0, 0, 10));
            }
        }
        #endregion
    }
}
 