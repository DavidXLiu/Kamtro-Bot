using Kamtro_Bot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public class UserDataManager
    {
        /// <summary>
        /// This Dictionary holds data for all of the users.
        /// The key is the user's Discord ID (unsigned long, unchangable)
        /// The value is a node with the user's basic data.
        /// So no Inventory data.
        /// 
        /// -C
        /// </summary>
        public static Dictionary<ulong, UserDataNode> UserData;

        /// <summary>
        /// This feature is not yet implemented, and is currently just a placeholder.
        /// This Dictionary hold data on the users inventories.
        /// The key is the user's Discord ID (unsigned long, unchangable)
        /// The value is an object with the user's inventory in it.
        /// 
        /// -C
        /// </summary>
        // public Dictionary<ulong, UserInventoryNode> UserInventories;


    }
}
