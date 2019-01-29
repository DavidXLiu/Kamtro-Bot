using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
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

        public static Dictionary<ulong, List<EventQueueNode>> EventQueue;

        public ReactionHandler(DiscordSocketClient client) {
            client.ReactionAdded += HandleReactionAsync;
            client.ReactionRemoved += HandleReactionAsync;

            EventQueue = new Dictionary<ulong, List<EventQueueNode>>();
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel channel, SocketReaction reaction) {
            if (reaction.User.Value.IsBot) return;  // More Robophobia (no bots allowed)

            List<EventQueueNode> awaitingActions = EventQueue[reaction.User.Value.Id];  // Get a list of the user's actions awaiting a reaction

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

        /// <summary>
        /// Adds an interface to the event queue
        /// </summary>
        /// -C
        /// <param name="embed">The interface to add</param>
        /// <param name="id">The ID of the user it's being added to</param>
        public static void AddEvent(KamtroEmbedBase embed, ulong id) {
            if (EventQueue.ContainsKey(id)) {
                // If the user is in the queue
                EventQueue[id].Add(new EventQueueNode(embed));  // Add the action to their list
            } else {
                // otherwise
                EventQueue.Add(id, new List<EventQueueNode>());  // Create the list
                EventQueue[id].Add(new EventQueueNode(embed));  // And add the action to their list
            }
        }

        /// <summary>
        /// Removes an event from the queue given it's embed
        /// </summary>
        /// <remarks>
        /// -C
        /// </remarks>
        /// <param name="id">The ID of the user who has the event</param>
        /// <param name="node">The node to remove</param>
        public static void RemoveEvent(KamtroEmbedBase node, ulong id) {
            foreach(EventQueueNode e in EventQueue[id]) {
                if(e.EventAction == node) {
                    EventQueue[id].Remove(e);
                    break;
                }
            }
        }
    }
}
