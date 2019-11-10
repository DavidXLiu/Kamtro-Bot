using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Items
{
    public class ItemPizza : Item, IUsable
    {
        public async Task Use(SocketGuildUser user, params object[] args) {
            UserDataManager.GetUserData(user).ReputationToGive += UserDataManager.GetUserData(user).MaxReputation;  // give rep
            UserInventoryManager.GetInventory(user.Id).LoseItem(Id);  // consume the item
            UserInventoryManager.SaveInventories(); // now save

            await NotifyChannel($"{user.GetDisplayName()} used a Pizza giving them {UserDataManager.GetUserData(user).MaxReputation} single-use extra rep points to give!");
        }
    }
}
