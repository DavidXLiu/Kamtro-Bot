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
        public int BuyPrice;
        public bool Available;  // If the item is available in the shop

        public ItemInfoNode(string name = "Unnamed Item", string desc = "Report to carbon or arcy.", bool avail = false, int price = 0) {
            // TODO
            Name = name;
            Description = desc;
            Available = avail;
            BuyPrice = price;
        }
    }
}
