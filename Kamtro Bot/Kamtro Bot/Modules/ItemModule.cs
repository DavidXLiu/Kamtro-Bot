using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Discord.Commands;
using Kamtro_Bot.Interfaces.ActionEmbeds;
using Kamtro_Bot.Items;
using Kamtro_Bot.Util;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Modules
{
    public class ItemModule : ModuleBase<SocketCommandContext>
    {
        private uint ItemId;
        private int Count;

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
            if (!Program.Experimental) return;
            // TODO: add feature where admins can look through a user's inventory
            InventoryEmbed ie = new InventoryEmbed(Context);
            await ie.Display();
        }
        #endregion
        #region Admin Commands
        [Command("refreshshop")]
        [Alias("refresh shop", "rfsh")]
        public async Task RefreshShopAsync([Remainder] string args = "") {
            if (!Program.Experimental) return;
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;
            ShopManager.GenShopSelection();
            await ReplyAsync(BotUtils.KamtroText("Shop Refreshed."));
        }

        [Command("giveitem")]
        [Alias("gi", "give")]
        public async Task GiveItemAsync([Remainder] string args = "") {
            if (!Program.Experimental) return;
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;

            List<string> data = args.Split(' ').ToList();
            int id_ind = data.Count - 2;
            int count_ind = data.Count - 1;

            uint id;
            int count;

            if (!uint.TryParse(data[id_ind], out id) || ItemManager.GetItem(ItemId) == null) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a valid item ID"));
                return;
            }

            if(!int.TryParse(data[count_ind], out count)) {
                await ReplyAsync(BotUtils.KamtroText("The count must be over 0!"));
                return;
            }

            ItemId = id;
            Count = count;

            data.RemoveRange(id_ind, 2);

            string search = "";

            foreach(string s in data) {
                search += $"{s} ";
            }

            List<SocketGuildUser> users = BotUtils.GetUsers(search.Trim(' '));

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await GiveItem(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, GiveItem, BotUtils.GetGUser(Context));
                await use.Display(Context.Channel);
            }
        }

        #region Helper Methods
        private async Task GiveItem(SocketGuildUser user) {
            UserInventoryManager.GetInventory(user.Id).AddItem(ItemId, Count);
            UserInventoryManager.SaveInventories();

            await ReplyAsync(BotUtils.KamtroText($"Successfully given {user.GetDisplayName()} {Count} of {ItemManager.GetItem(ItemId).Name}"));
        }
        #endregion
        #endregion
    }
}
