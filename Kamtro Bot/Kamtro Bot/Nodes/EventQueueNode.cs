using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kamtro_Bot.Interfaces;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// Node for the event queue.
    /// </summary>
    /// -C
    public class EventQueueNode
    {
        public DateTime TimeCreated;  // This is for the timeout and garbage collection thread
        public KamtroEmbedBase EventAction;  // This is the action that the node is for

        public EventQueueNode(KamtroEmbedBase action) {
            EventAction = action;
        }
    }
}
