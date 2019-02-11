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


        /// <summary>
        /// Adds an interface to the event queue
        /// </summary>
        /// -C
        /// <param name="embed">The interface to add</param>
        public static void AddEvent(ActionEmbed embed) {
            ulong id = embed.CommandCaller.Id;

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
    }
}
