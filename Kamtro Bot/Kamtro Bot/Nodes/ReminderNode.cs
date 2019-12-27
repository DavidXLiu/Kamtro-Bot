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
       public string Description;

        public ReminderNode(string name, string desc) {
            Name = name;
            Description = desc;
        }
    }
}
