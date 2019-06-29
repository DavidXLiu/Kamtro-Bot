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
        public bool IsAlone = false; // This tells other classes if this object is alone (AKA in an action event), or is paired up with another type of event node (AKA in a message event)

        public EventQueueNode(ActionEmbed action, bool isAlone = false) {
            IsAlone = isAlone;
            EventAction = action;
            TimeCreated = DateTime.Now;
        }
    }
}
