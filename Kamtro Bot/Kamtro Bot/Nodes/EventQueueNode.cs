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
        public ActionEmbed EventAction;  // This is the action that the node is for

        public EventQueueNode(ActionEmbed action) {
            EventAction = action;
            TimeCreated = DateTime.Now;
        }
    }
}
