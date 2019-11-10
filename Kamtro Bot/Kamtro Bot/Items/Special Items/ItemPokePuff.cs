using Discord.WebSocket;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ItemPokePuff : Item, IUsable
    {
        public async Task Use(SocketGuildUser user, params object[] args) {
            UserDataManager.GetUserData(user).ReputationToGive += 1;  // give rep
            UserInventoryManager.GetInventory(user.Id).LoseItem(Id);  // consume the item
            UserInventoryManager.SaveInventories(); // now save

            await NotifyChannel($"{user.GetDisplayName()} Used a Poke-Puff, giving them 1 single-use extra rep point to give!");
        }
    }
}
