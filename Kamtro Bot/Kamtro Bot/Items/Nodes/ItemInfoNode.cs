using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ItemInfoNode
    {
        public string Name;
        public string Description;
        public bool Available;  // If the item is available in the shop
        
        public ItemInfoNode() {
            // TODO
            Name = "";
            Description = "";
            Available = false;
        }
    }
}
