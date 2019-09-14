using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    /// <summary>
    /// Contains and manages the event queue
    /// </summary>
    public class EventQueueManager
    {
        public static Dictionary<ulong, List<EventQueueNode>> EventQueue = new Dictionary<ulong, List<EventQueueNode>>();
        public static Dictionary<ulong, MessageEventNode> MessageEventQueue = new Dictionary<ulong, MessageEventNode>();

        #region Event Queue
        /// <summary>
        /// Adds an interface to the event queue
        /// </summary>
        /// -C
        /// <param name="embed">The interface to add</param>
        public static void AddEvent(ActionEmbed embed) {
            ulong id = embed.CommandCaller.Id;

            bool alone = embed is MessageEmbed;

            if (EventQueue.ContainsKey(id)) {
                // If the user is in the queue
                EventQueue[id].Add(new EventQueueNode(embed, alone));  // Add the action to their list
            } else {
                // otherwise
                EventQueue.Add(id, new List<EventQueueNode>());  // Create the list
                EventQueue[id].Add(new EventQueueNode(embed, alone));  // And add the action to their list
            }
        }

        /// <summary>
        /// Removes an event from the queue given it's embed
        /// </summary>
        /// <remarks>
        /// -C
        /// </remarks>
        /// <param name="node">The node to remove</param>
        public static void RemoveEvent(ActionEmbed node) {
            ulong id = node.CommandCaller.Id;

            foreach (EventQueueNode e in EventQueue[node.CommandCaller.Id]) {
                if (e.EventAction == node) {
                    EventQueue[id].Remove(e);
                    break;
                }
            }
        }
        #endregion
        #region Message Event Queue
        /// <summary>
        /// Adds a message embed to the queue.
        /// </summary>
        /// <remarks>
        /// All methods relating to the Message Event Queue are O(1) constant time.
        /// This is because there is no list of events as there can only be one at a time for a user.
        /// </remarks>
        /// <param name="embed">The embed to add</param>
        public static void AddMessageEvent(MessageEmbed embed) {
            ulong id = embed.CommandCaller.Id;

            if (MessageEventQueue.ContainsKey(id)) {
                // If the user is in the queue
                MessageEventQueue[id] = new MessageEventNode(embed, embed.CommandChannel);  // Add the action to their list
            } else {
                // otherwise
                MessageEventQueue.Add(id, new MessageEventNode(embed, embed.CommandChannel));  // Create the entry and add the action to their list
            }
        }

        /// <summary>
        /// Removes a message event from the queue.
        /// </summary>
        /// <param name="node">The embed to remove.</param>
        public static void RemoveMessageEvent(MessageEmbed node) {
            ulong id = node.CommandCaller.Id;

            RemoveMessageEvent(id);
        }

        /// <summary>
        /// Removes a message event from the queue via ID
        /// </summary>
        /// <param name="id">The ID of the user associated with the node</param>
        public static void RemoveMessageEvent(ulong id) {
            MessageEventQueue.Remove(id);
            EventQueue.Remove(id);
        }

        /// <summary>
        /// Tests to see if a user has a pending event
        /// </summary>
        /// <param name="user">The user to test</param>
        /// <returns>true if the user has an event waiting on them, false otherwise</returns>
        public static bool HasMessageEvent(SocketUser user) {
            return MessageEventQueue[user.Id] != null;
        }
        #endregion
    }
}
