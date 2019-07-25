using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// The handler class for reactions.
    /// Holds utility constants, and has the event handler method for reaction add/remove events.
    /// </summary>
    /// <remarks>
    /// Mostly just used for the interfaces
    /// 
    /// TODO:
    /// - Add #regions for the constants
    /// 
    /// -C
    /// </remarks>
    public class ReactionHandler
    {
        /**
         * These variables are helpful constants for interface creation.
         * Instead of having to make a new MenuOptionNode every time, you can now just use one of these constants.
         * Each block of text here (seperated by a space in this context) contains a different object relating to the emoji.
         * 
         * First block is the emoji in string form. We should use proper excape sequences for this,
         * and not use the emoji character as this has caused issues in the past.
         * 
         * Second block is the emoji object. Useful for putting in text.
         * 
         * Third block is for the MenuOptionNodes, very useful for quick creation of menus.
         * 
         * -C
         */
        public const string DONE_STR = "🔚";
        public const string CHECK_STR = "\u2705";
        public const string DECLINE_STR = "\u274C";
        public const string UP_STR = "\u2b06";
        public const string DOWN_STR = "\u2b07";
        public const string SELECT_STR = "\U0001f537";

        public static readonly Emoji DONE_EM = new Emoji(DONE_STR);  // For utility
        public static readonly Emoji CHECK_EM = new Emoji(CHECK_STR);
        public static readonly Emoji DECLINE_EM = new Emoji(DECLINE_STR);
        public static readonly Emoji UP_EM = new Emoji(UP_STR);
        public static readonly Emoji DOWN_EM = new Emoji(DOWN_STR);
        public static readonly Emoji SELECT_EM = new Emoji(SELECT_STR);

        public static readonly MenuOptionNode DONE = new MenuOptionNode(DONE_STR, "Done");  // This is also for convinience
        public static readonly MenuOptionNode CHECK = new MenuOptionNode(CHECK_STR, "Confirm");
        public static readonly MenuOptionNode DECLINE = new MenuOptionNode(DECLINE_STR, "Cancel");
        public static readonly MenuOptionNode UP = new MenuOptionNode(UP_STR, "Cursor Up");
        public static readonly MenuOptionNode DOWN = new MenuOptionNode(DOWN_STR, "Cursor Down");
        public static readonly MenuOptionNode SELECT = new MenuOptionNode(SELECT_STR, "Select");

        public static Dictionary<string, ulong> RoleMap = new Dictionary<string, ulong>();

        /// <summary>
        /// Constructor for the handler, called on startup. 
        /// This creates a handler object, and assigns it to handle the reaction events.
        /// </summary>
        /// <param name="client">The Client Object</param>
        public ReactionHandler(DiscordSocketClient client) {
            SetupDict();

            client.ReactionAdded += HandleReactionAsync;
            client.ReactionRemoved += HandleReactionAsync;
            client.ReactionRemoved += RemoveReaction;
        }

        #region Events
        /// <summary>
        /// Reaction add/remove event handler method.
        /// </summary>
        /// <remarks>
        /// This is called every time a reaction is added or removed, basically every time a user
        /// clicks the reaction button.
        /// </remarks>
        /// <param name="cacheableMessage">Message that the reaction was added to</param>
        /// <param name="channel">The channel the message with the reaction is in</param>
        /// <param name="reaction">The reaction added</param>
        /// <returns></returns>
        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel channel, SocketReaction reaction) {
            if (reaction.User.Value.IsBot) return;  // More Robophobia (no bots allowed)

            if ((await cacheableMessage.GetOrDownloadAsync()).Id == Program.Settings.RoleSelectMessageID && channel as SocketTextChannel != null) {
                // if it's the role select message
                await OnRoleMessageReaction(channel as SocketTextChannel, reaction);
                return;
            }

            List<EventQueueNode> awaitingActions = EventQueueManager.EventQueue[reaction.User.Value.Id];  // Get a list of the user's actions awaiting a reaction

            foreach (EventQueueNode action in awaitingActions) {
                if (DateTime.Now - action.TimeCreated > BotUtils.Timeout) continue;  // If the GC is going to clean it up, don't risk a race condition.
                if (cacheableMessage.Value.Id == action.EventAction.Message.Id) {
                    // If the message matches the one in the Embed
                    if (reaction.Emote.ToString() == DONE_EM.ToString()) {
                        // The user has indicated that they no longer need the Interface,
                        // So remove it from the dict
                        awaitingActions.Remove(action);
                        return;  // Also exit the method
                    }

                    await action.EventAction.PerformAction(reaction);  // Do the action with the reaction specified
                }
            }
        }

        private async Task RemoveReaction(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
            if ((await msg.GetOrDownloadAsync()).Id == Program.Settings.RoleSelectMessageID && channel as SocketTextChannel != null && RoleMap.ContainsKey(reaction.Emote.Name)) {
                SocketTextChannel chan = channel as SocketTextChannel;
                // this method only checks for this message. Ignore in other cases
                if (chan.Guild.GetUser(reaction.UserId).Roles.Contains(chan.Guild.GetRole(RoleMap[reaction.Emote.Name]))) {
                    await chan.Guild.GetUser(reaction.UserId).RemoveRoleAsync(chan.Guild.GetRole(RoleMap[reaction.Emote.Name]));
                }
            }
        }
        #endregion
        #region Helper Methods
        private async Task OnRoleMessageReaction(SocketTextChannel channel, SocketReaction reaction) { 
            if (RoleMap.ContainsKey(reaction.Emote.Name)) {
                SocketRole role = ServerData.Server.GetRole(RoleMap[reaction.Emote.Name]);

                if (channel.Guild.GetUser(reaction.UserId).Roles.Contains(role)) return; // if they already have the role, don't bother giving it to them again

                await channel.Guild.GetUser(reaction.UserId).AddRoleAsync(role);
            }
        }

        private void SetupDict() {
            RoleMap.Add("OwO", 535627110153191427);
        }
        #endregion
    }
}
