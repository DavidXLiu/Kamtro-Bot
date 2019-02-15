using Kamtro_Bot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// For the Message Event Queue
    /// </summary>
    /// <remarks>
    /// This class is a node class that stores info on the message that it's waiting on.
    /// Currently not useful B/C it's only real use would be to store a pointer to the object. 
    /// </remarks>
    public class MessageEventNode
    {
        public MessageEmbed Interface;
        public DateTime TimeCreated;

        public MessageEventNode(MessageEmbed iface) {
            Interface = iface;
            TimeCreated = DateTime.Now;
        }
    }
}
