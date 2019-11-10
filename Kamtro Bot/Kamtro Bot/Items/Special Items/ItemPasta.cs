using Discord.WebSocket;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ItemPasta : Item, IUsable
    {
        public async Task Use(SocketGuildUser user, params object[] args) {
            UserDataManager.GetUserData(user).ReputationToGive += 5;  // give rep
            UserInventoryManager.GetInventory(user.Id).LoseItem(Id);  // consume the item
            UserInventoryManager.SaveInventories(); // now save

            await NotifyChannel($"{user.GetDisplayName()} Used Angel Hair Pasta, giving them 5 single-use extra rep points to give!");
        }
    }
}
