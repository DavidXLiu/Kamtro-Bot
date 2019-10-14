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
using Kamtro_Bot.Interfaces;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// Handler for all events that do not need their own class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class GeneralHandler
    {
        private const int MESSAGE_WARNING_COUNT = 5;
        private const int MESSAGE_BAN_COUNT = 7;
        private const int MESSAGE_TIME = 5;

        public static Dictionary<ulong, CrossBanDataNode> CrossBan;

        public static bool ResetOn = true;

        public static Dictionary<ulong, int> ConsMessages = new Dictionary<ulong, int>();
        public static Dictionary<ulong, ulong> LastUser = new Dictionary<ulong, ulong>();

        #region Event Handlers
        public GeneralHandler(DiscordSocketClient client) {
            // Setup
            LoadList();

            // Add the events
            client.GuildMemberUpdated += OnMemberUpdate;
            client.UserJoined += OnMemberJoin;
            client.UserBanned += OnMemberBan;
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
            // Add default roles
            await user.AddRoleAsync(ServerData.Kamexican);
            await user.AddRoleAsync(ServerData.Retropolitan);


            // For cross ban.
            if(CrossBan != null && CrossBan.ContainsKey(user.Id)) {
                await BotUtils.AdminLog($"Cross-banned user {BotUtils.GetFullUsername(user)}. " + CrossBan[user.Id].GetInfoText());
                AdminDataManager.AddBan(user, new BanDataNode(Program.Client.CurrentUser, $"[X-ban | {CrossBan[user.Id].GetServer()}] {CrossBan[user.Id].Reason}"));
                await ServerData.Server.AddBanAsync(user);

                KLog.Info($"Cross-banned user {BotUtils.GetFullUsername(user)}");
                return;
            }
            // welcome user 
            Embed e = new EmbedBuilder().WithTitle("Welcome to Kamtro!").WithColor(BotUtils.Kamtro).WithDescription(Program.Settings.WelcomeMessageTemplate).Build();
            await BotUtils.DMUserAsync(user, e);
        }

        /// <summary>
        /// Happens when a user is banned from a server the bot is in.
        /// </summary>
        /// <param name="user">The user that was banned</param>
        /// <param name="server">The server they were banned from</param>
        /// <returns></returns>
        public async Task OnMemberBan(SocketUser user, SocketGuild server)
        {
            // Store last user banned
            ServerData.BannedUser = user;
        }

        /// <summary>
        /// Happens when a user is unbanned from a server the bot is in.
        /// </summary>
        /// <param name="user">The user that was unbanned</param>
        /// <param name="server">The server they were unbanned from</param>
        /// <returns></returns>
        public async Task OnMemberUnban(SocketUser user, SocketGuild server) {
            if(CrossBan == null) {
                CrossBan = new Dictionary<ulong, CrossBanDataNode>();
                SaveList();
            }

            if (CrossBan.ContainsKey(user.Id) && server.Id == ServerData.Server.Id) {
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
            if (before.Guild != ServerData.Server || before.Status != after.Status) return; // If it's not on kamtro, or it was just a status update (online to AFK, etc), ignore it

            KLog.Debug($"Member {BotUtils.GetFullUsername(before)} updated.");

            if(BotUtils.GetFullUsername(before) != BotUtils.GetFullUsername(after)) {
                // If the user changed their name.
                KLog.Debug($"Username Change: {BotUtils.GetFullUsername(before)} --> {BotUtils.GetFullUsername(after)}");
                
                NameChangeEmbed nce = new NameChangeEmbed(before, after);
                await nce.Display(ServerData.LogChannel, after.Username + "#" + after.Discriminator);
            }

            KLog.Debug("FLAG GH-A");

            if (before.Nickname != after.Nickname) {
                // If the nickame changed
                KLog.Debug($"Nickname Change: {BotUtils.GetFullUsername(before)}: {(before.Nickname == null ? "[No Nickname]":$"'{before.Nickname}'")} --> {(after.Nickname == null ? "[No Nickname]" : $"'{after.Nickname}'")}");

                NameChangeEmbed nce = new NameChangeEmbed(before, after, true);
                await nce.Display(ServerData.LogChannel, after.Username + "#" + after.Discriminator);
            }

            KLog.Debug("FLAG GH-B");

            if (before.GetAvatarUrl() != after.GetAvatarUrl()) {
                KLog.Debug($"Avatar change from {BotUtils.GetFullUsername(after)}");
                // KLog.Debug($"");
               AvatarUpdateEmbed emb = new AvatarUpdateEmbed(before, after);
                await emb.Display(ServerData.LogChannel, after.Username + "#" + after.Discriminator);
            }
            KLog.Debug("FLAG GH-C ");
            if (before.Roles.Count != after.Roles.Count) {
                // role update
                KLog.Debug($"Role Update user {BotUtils.GetFullUsername(after)}");
                if (!before.Roles.Contains(ServerData.Lurker) && after.Roles.Contains(ServerData.Lurker)) return; // if it was just a lurker update, don worry bout it.  -C
                KLog.Debug("FLAG GH-D");
                if(after.Roles.Contains(ServerData.Lurker) && (after.Roles.Contains(ServerData.Kamexican) || after.Roles.Contains(ServerData.Retropolitan))) {
                    KLog.Debug("FLAG GH-E");
                    await after.RemoveRoleAsync(ServerData.Lurker); 
                }
                KLog.Debug("FLAG GH-F");
                // for lurker
                if (!after.Roles.Contains(ServerData.Kamexican) && !after.Roles.Contains(ServerData.Retropolitan) && !after.Roles.Contains(ServerData.Lurker)) {
                    KLog.Debug("FLAG GH-G");
                    await after.AddRoleAsync(ServerData.Lurker);
                    KLog.Debug("FLAG GH-H");
                    return;
                }
                KLog.Debug("FLAG GH-I");

                // for nsfw remove
                foreach (SocketRole role in after.Roles) {
                    if(ServerData.SilencedRoles.Contains(role)) {
                        // remove mature role if user has it
                        if(after.Roles.Contains(ServerData.NSFWRole)) {
                            KLog.Debug("FLAG GH-J");
                            await after.RemoveRoleAsync(ServerData.NSFWRole);
                        }
                    }
                }
            }

            KLog.Debug("FLAG GH-Z");
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
            await messageEdit.Display(ServerData.LogChannel);
        }
        
        /// <summary>
        /// Called when a user deletes a message
        /// </summary>
        /// <param name="message">The deleted message</param>
        /// <param name="channel">The channel it was deleted in</param>
        /// <returns></returns>
        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) {
            IMessage msg = await message.GetOrDownloadAsync();

            if (msg != null && (msg.Source != MessageSource.User || !(msg is SocketMessage) || !(channel is SocketTextChannel))) return;

            SocketMessage sm = msg as SocketMessage;
            MessageDeleteEmbed mde;

            if (sm == null) {
                mde = new MessageDeleteEmbed(null, channel: channel as SocketTextChannel);
            } else {
                mde = new MessageDeleteEmbed(sm);
            }

            await mde.Display(ServerData.LogChannel);
        }
        #endregion

        #region Non-Handlers
        public static async Task HandleMessage(SocketUserMessage msg) {
            if (msg.Channel is ISocketPrivateChannel || msg.Channel.Id == Program.Settings.BotChannelID) return;  // don't autoban for bot channel

            if(!LastUser.ContainsKey(msg.Channel.Id)) {
                LastUser.Add(msg.Channel.Id, 0);
            }

            if(!ConsMessages.ContainsKey(msg.Channel.Id)) {
                ConsMessages.Add(msg.Channel.Id, 0);
            }

            SocketGuildUser auth = BotUtils.GetGUser(msg.Author.Id);

            if (auth == null) return;

            if (LastUser[msg.Channel.Id] == 0 || auth.Id != LastUser[msg.Channel.Id]) {
                LastUser[msg.Channel.Id] = msg.Author.Id;
                ConsMessages[msg.Channel.Id] = 1;
            } else {
                ConsMessages[msg.Channel.Id]++;
            }
            
            if(ConsMessages[msg.Channel.Id] == MESSAGE_WARNING_COUNT && !ServerData.HasPermissionLevel(auth, ServerData.PermissionLevel.TRUSTED)) {
                await msg.Channel.SendMessageAsync(BotUtils.KamtroAngry($"Please slow down {BotUtils.GetFullUsername(msg.Author)}, or you will be autobanned for spam."));
            }

            if(ConsMessages[msg.Channel.Id] >= MESSAGE_BAN_COUNT && !ServerData.HasPermissionLevel(auth, ServerData.PermissionLevel.TRUSTED)) {
                await BotUtils.AdminLog($"User {BotUtils.GetFullUsername(auth)} was autobanned for spam");
                await ServerData.Server.AddBanAsync(auth);

                AdminDataManager.AddBan(msg.Author, new BanDataNode(Program.Client.CurrentUser, "Autobanned for spam."));

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

                // Must make a list of the keys to prevent exception of collection change in a loop
                List<ulong> keys = ConsMessages.Keys.ToList();
                foreach(ulong key in keys) {
                    ConsMessages[key] = 0;
                }

                Thread.Sleep(new TimeSpan(0, 0, MESSAGE_TIME));
            }
        }
        #endregion
    }
}
 