using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// Node class to store info on the menu options of the interface.
    /// </summary>
    /// -C
    public class MenuOptionNode
    {
        public string Icon;  // This is the Reaction emoji that the option will use (C# strings are UTF-16 compadible, praise Microsoft)
        public string Description;  // This is the description of what the Icon actually does.

        /// <summary>
        /// Constructs a MenuOptionNode
        /// </summary>
        /// <param name="icon">The icon as an emoji</param>
        /// <param name="desc">The description of what the option does</param>
        public MenuOptionNode(string icon, string desc) {
            Icon = icon;
            Description = desc;
        }
    }
}
