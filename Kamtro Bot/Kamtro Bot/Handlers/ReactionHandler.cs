﻿using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// The handler class for reactions
    /// </summary>
    /// <remarks>
    /// Mostly just used for the interfaces
    /// -C
    /// </remarks>
    public class ReactionHandler
    {
        public const string DONE_STR = "🔚";  // This is a constant for the done button
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




        public ReactionHandler(DiscordSocketClient client) {
            client.ReactionAdded += HandleReactionAsync;
            client.ReactionRemoved += HandleReactionAsync;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel channel, SocketReaction reaction) {
            if (reaction.User.Value.IsBot) return;  // More Robophobia (no bots allowed)

            List<EventQueueNode> awaitingActions = EventQueueManager.EventQueue[reaction.User.Value.Id];  // Get a list of the user's actions awaiting a reaction

            foreach(EventQueueNode action in awaitingActions) {
                if (DateTime.Now - action.TimeCreated > BotUtils.Timeout) continue;  // If the GC is going to clean it up, don't risk a race condition.
                if(cacheableMessage.Value.Id == action.EventAction.Message.Id) {
                    // If the message matches the one in the Embed
                    if(reaction.Emote.ToString() == DONE_EM.ToString()) {
                        // The user has indicated that they no longer need the Interface,
                        // So remove it from the dict
                        awaitingActions.Remove(action);
                        return;  // Also exit the method
                    }

                    await action.EventAction.PerformAction(reaction);  // Do the action with the reaction specified
                }
            }
        }
    }
}
