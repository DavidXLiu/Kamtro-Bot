using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Items
{
    public class ItemBreadstick : Item, IUsable
    {
        public void Use(SocketGuildUser user, params object[] args) {
            UserDataManager.GetUserData(user).ReputationToGive += 3;  // give rep
            UserInventoryManager.GetInventory(user.Id).LoseItem(Id);  // consume the item
            UserInventoryManager.SaveInventories(); // now save
        }
    }
}
