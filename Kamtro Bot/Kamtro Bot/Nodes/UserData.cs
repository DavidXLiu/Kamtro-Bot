using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is the node that will contain a user's stats
    /// </summary>
    public class UserDataNode
    {
        public int Score;  // Message score  -C
        public int Reputation;  // Reputation points  -C
        public int Money;  // Kamtrokens  -C
        public TitleNode CurrentTitle;  // The user's selected title.  -C
        public List<TitleNode> Titles;  // A list of titles the user has  -C
        public int Strikes;  // The number of strikes a user has  -C
    }
}
