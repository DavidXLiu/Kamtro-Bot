using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Discord.Commands;
using Kamtro_Bot.Interfaces.ActionEmbeds;
using Kamtro_Bot.Items;

namespace Kamtro_Bot.Modules
{
    public class ItemModule : ModuleBase<SocketCommandContext>
    {
        #region User Commands
        [Command("shop")]
        public async Task ShopAsync([Remainder] string args = "") {
            if (!Program.Experimental) return;

            ShopEmbed shop = new ShopEmbed(Context);
            await shop.Display();
        }

        [Command("craft")]
        public async Task CraftAsync([Remainder] string args = "") {
            if (!Program.Experimental) return;
            CraftingEmbed cre = new CraftingEmbed(Context);
            await cre.Display();
        }

        [Command("inventory")]
        [Alias("inv")]
        public async Task InventoryAsync([Remainder] string args = "") {
            // TODO: add feature where admins can look through a user's inventory

            if (!Program.Experimental) return;

            InventoryEmbed ie = new InventoryEmbed(Context);
            await ie.Display();
        }
        #endregion
        #region Admin Commands
        [Command("refreshshop")]
        [Alias("refresh shop", "rfsh")]
        public async Task RefreshShopAsync([Remainder] string args = "") {
            ShopManager.GenShopSelection();
            await ReplyAsync(BotUtils.KamtroText("Shop Refreshed."));
        }
        #endregion
    }
}
