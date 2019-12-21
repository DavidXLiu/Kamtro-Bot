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
        private int ItemCount = 1;
        private uint? SelectedItem = null;

        private bool ConfirmOut = false;  // protect against spam

        private bool ZeroItemCountWarn = false;
        private bool CantCraftWarn = false;

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
            eb.WithColor(Item.GetColorFromRarity(GetItemAtCursor().Rarity));
            eb.WithThumbnailUrl(GetItemAtCursor().GetImageUrl());

            if (SelectedItem != null) {
                eb.WithThumbnailUrl(GetItemAtCursor().GetImageUrl());
            }

            if (Page == HOME_PAGE) {
                eb.WithDescription("Items you have enough materials to craft are in bold.");

                string list = "";

                for (int i = 0; i < CraftItems.Count; i++) {
                    list += MakeBold($"{(i == Cursor ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{ItemManager.GetItem(CraftItems[i]).Name}", Inventory.CanCraft(CraftItems[i])) + "\n";
                }

                if (CraftItems.Count == 0) list = "Crafting Unavailable!";

                eb.AddField("Crafting Options", list.Trim('\n'));

                // Menu
                ClearMenuOptions();
                AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.BACK);
            } else {
                eb.WithDescription("Crafting ingredients marked in bold are ones you have enough of to craft this item.");

                eb.AddField("Name", GetItemAtCursor().Name);

                // Crafting Cost
                string cost = "";

                foreach (uint i in GetItemAtCursor().GetRecipe().Keys) {
                    int owned = Inventory.ItemCount(ItemManager.GetItem(i));  // number of the ingredient owned by the user
                    int needed = GetItemAtCursor().GetRecipe()[i];

                    cost += MakeBold($"{ItemManager.GetItem(i).Name} ({owned}/{needed*ItemCount})", owned >= needed*ItemCount) + "\n";
                }

                eb.AddField("Crafting Cost (Owned/Needed)", cost);

                eb.AddField("Description", GetItemAtCursor().Description);

                // To Craft Count
                eb.AddField("Item Count", $"{ItemCount}");

                // Custom Menu
                ClearMenuOptions();
                AddMenuOptions(ReactionHandler.SELECT, new MenuOptionNode(ReactionHandler.UP_STR, "Increase Crafting Count"), new MenuOptionNode(ReactionHandler.DOWN_STR, "Decrease Crafting Count"), ReactionHandler.BACK);
            }

            string warnings = "";

            // Warnings
            if(CantCraftWarn) {
                warnings += "You can't craft that many of this item!\n";
                CantCraftWarn = false;
            }

            if(ZeroItemCountWarn) {
                warnings += "You can't craft 0 items!\n\n";
                ZeroItemCountWarn = false;
            }

            if (!string.IsNullOrWhiteSpace(warnings)) eb.AddField("Warning", warnings.Trim('\n'));

            AddMenu(eb); // menu only makes sense on home page, adjust it for the other pages

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
                        await UpdateEmbed();
                    } else {
                        // item page
                        if(ItemCount == 0) {
                            ZeroItemCountWarn = true;  // if the number to craft is 0, notify the user that they can only craft one or more of an item.
                            await UpdateEmbed();
                        } else {
                            if(Inventory.CanCraft(GetItemAtCursor().Id, ItemCount) && !ConfirmOut) {
                                // User can craft X items
                                ConfirmOut = true;
                                ConfirmEmbed ce = new ConfirmEmbed(Context, $"Are you sure you want to craft {ItemCount} item{(ItemCount == 1 ? "" : "s")}?", DoCraft);
                                await ce.Display();
                            } else {
                                // user can't craft
                                CantCraftWarn = true;
                                await UpdateEmbed();
                            }
                        }
                    }
                    break;

                case ReactionHandler.BACK_STR:
                    if(Page != HOME_PAGE) {
                        Page = HOME_PAGE;
                        await UpdateEmbed();
                    }
                    break;
            }
        }

        private async Task CursorUp() {
            if (Page == HOME_PAGE) {
                Cursor--;

                if (Cursor < 0) Cursor = CraftItems.Count() - 1;

                SelectedItem = CraftItems[Cursor];
            } else {
                ItemCount++;
            }

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (Page == HOME_PAGE) {
                Cursor++;

                if (Cursor >= CraftItems.Count()) Cursor = 0;

                SelectedItem = CraftItems[Cursor];
            } else {
                ItemCount--;

                if (ItemCount < 0) ItemCount = 0;
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

        private async Task DoCraft(bool craft) {
            for (int i = 0; i < ItemCount; i++) {
                Inventory.TryCraft(SelectedItem.Value);
            }

            ConfirmOut = false;

            await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Successfully crafted {GetItemAtCursor().Name} * {ItemCount}"));
        }

        private string MakeBold(string s, bool b) {
            if(b) {
                return $"**{s}**";
            }

            return s;
        }
    }
}
