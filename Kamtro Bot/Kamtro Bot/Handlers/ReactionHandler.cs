using Discord;
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
        public static readonly Emoji DONE = new Emoji(DONE_STR);  // For utility
        public static readonly MenuOptionNode DONE_NODE = new MenuOptionNode(DONE_STR, "Done");  // This is also for convinience

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
                    if(reaction.Emote.ToString() == DONE.ToString()) {
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
