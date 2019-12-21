using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class ReminderNode
    {
        public string Name;
        public DateTime Date;
        public string Description;

        public ReminderNode(string name, DateTime date, string desc) {
            Name = name;
            Date = date;
            Description = desc;
        }

        /// <summary>
        /// Gets the time until the reminder needs to be sent
        /// </summary>
        /// <returns>A TimeSpan object representing the time until the user must be messages</returns>
        public TimeSpan GetTimeUntilRemind() {
            return Date - DateTime.UtcNow;
        }
    }
}
