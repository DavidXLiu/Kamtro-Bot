using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Items
{
    public class ItemBreadstick : Item, IUsable
    {
        public async Task Use(SocketGuildUser user, SocketCommandContext ctx, params object[] args) {
            UserDataManager.GetUserData(user).ReputationToGive += 3;  // give rep
            UserInventoryManager.GetInventory(user.Id).LoseItem(Id);  // consume the item
            UserInventoryManager.SaveInventories(); // now save

            await NotifyChannel(ctx, $"{user.GetDisplayName()} used a Breadstick giving them 3 single-use extra rep points to give!");
        }
    }
}
