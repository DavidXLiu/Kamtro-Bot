using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Discord.Commands;

namespace Kamtro_Bot.Modules
{
    public class ItemModule : ModuleBase<SocketCommandContext>
    {
        [Command("shop")]
        public async Task ShopAsync([Remainder] string args = "") {

        }

        [Command("craft")]
        public async Task CraftAsync([Remainder] string args = "") {

        }

        [Command("inventory")]
        public async Task InventoryAsync([Remainder] string args = "") {

        }
    }
}
