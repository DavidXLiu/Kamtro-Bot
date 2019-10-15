using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Items;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class CraftingEmbed : ActionEmbed
    {
        private const int HOME_PAGE  = -1;
        private const int CRAFT_PAGE = 0;

        private int Page = -1;
        private int Cursor = 0;
        private int ItemCount = 0;
        private uint? SelectedItem = null;

        private bool ZeroItemCountWarn = false;

        private SocketGuildUser User;
        private UserInventoryNode Inventory;

        private List<uint> CraftItems;

        public CraftingEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);
            Inventory = UserInventoryManager.GetInventory(User.Id);

            CraftItems = GetCraftableItems();

            if(CraftItems.Count > 0) {
                SelectedItem = CraftItems[0];
            }

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.BACK);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Crafting");

            if (SelectedItem != null) {
                eb.WithThumbnailUrl(GetItemAtCursor().GetImageUrl());
            }

            if (Page == HOME_PAGE) {
                /*
                    Home Page
                    Must have:
                        - List of craftable items
                        - Color is item rarity
                        - Image is item icon
                */
                
            } else {
                /*
                    Crafting Page
                    Must have:
                        - Item Name
                        - Crafting cost
                            - Each ingredient must have a count in the form of (a/b) where a is the number the user has, and b is the number needed to craft
                        - To Craft Count
                        - Item Description
                        - Color is item rarity
                        - Image is item icon
                */
            }

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch (option.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    await CursorUp();
                    break;

                case ReactionHandler.DOWN_STR:
                    await CursorDown();
                    break;

                case ReactionHandler.SELECT_STR:
                    if (Page == HOME_PAGE) {
                        Page = CRAFT_PAGE;
                    } else {
                        // item page
                        if(ItemCount == 0) {
                            ZeroItemCountWarn = true;  // if the number to craft is 0, notify the user that they can only craft one or more of an item.
                        } else {
                            // craft the item
                            for (int i = 0; i < ItemCount; i++) {
                                Inventory.TryCraft(SelectedItem.Value);
                            }
                        }
                    }

                    await UpdateEmbed();
                    break;

                case ReactionHandler.BACK_STR:

                    break;
            }
        }

        private async Task CursorUp() {
            if (Page == HOME_PAGE) {
                Cursor--;

                if (Cursor < 0) Cursor = CraftItems.Count() - 1;

                SelectedItem = CraftItems[Cursor];
            }

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (Page == HOME_PAGE) {
                Cursor++;

                if (Cursor >= CraftItems.Count() - 1) Cursor = 0;

                SelectedItem = CraftItems[Cursor];
            }

            await UpdateEmbed();
        }

        private Item GetItemAtCursor() {
            if (SelectedItem == null) return null;

            return ItemManager.GetItem(SelectedItem.Value);
        }

        private List<uint> GetCraftableItems() {
            List<uint> craftable = new List<uint>();

            foreach (uint k in ItemManager.Items.Keys) {
                if(ItemManager.GetItem(k).IsCraftable()) {
                    craftable.Add(k);
                }
            }

            craftable.Sort();

            return craftable;
        }
    }
}
