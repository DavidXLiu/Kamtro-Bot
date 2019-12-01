using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Items;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class InventoryEmbed : ActionEmbed
    {
        private const int MAX_ITEM_OPTIONS = 2;  // the number of things you can do with an item max (for now, sell and use)
        private const int HOME_PAGE = -1;
        private const int ITEM_PAGE = 0;
        private const int SELL_PAGE = 1;

        private SocketGuildUser User;
        private UserDataNode UserData;
        private UserInventoryNode Inventory;

        private uint? SelectedItem = null;
        private int Cursor = 0;
        private int ItCursor = 0;  // cursor to select sell or use. 0 for use, 1 for sell
        private int Page = -1;
        private int SellCount = 0;

        // Warning Flags
        private bool InvalidSellCount = false;

        public InventoryEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);
            UserData = UserDataManager.GetUserData(User);
            Inventory = UserInventoryManager.GetInventory(User.Id);

            List<uint> Items = Inventory.Items.Keys.ToList();
            Items.Sort();

            if (Items.Count != 0) SelectedItem = Items[0];

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.BACK, ReactionHandler.UP, ReactionHandler.DOWN);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"{User.GetDisplayName()}'s Inventory");
            eb.WithColor(UserData.ProfileColor);

            if(SelectedItem != null) {
                eb.WithThumbnailUrl(GetItemAtCursor().GetImageUrl());
            }

            if (Page == HOME_PAGE) {
                // if it's the home page
                string names = "";
                string count = "";

                if (Inventory.ItemCount() == 0) {
                    names = "You have no items!";
                } else {
                    List<uint> Items = Inventory.Items.Keys.ToList();
                    Items.Sort();

                    for (int i = 0; i < Items.Count; i++) {
                        Item it = ItemManager.GetItem(Items[i]);

                        if (i == Cursor) {
                            eb.WithThumbnailUrl(it.GetImageUrl());
                        }

                        names += $"{(i == Cursor ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{it.Name}\n";
                        count += $"{MakeBold(Inventory.ItemCount(Items[i]).ToString(), i)}\n";
                    }
                }

                eb.AddField("Items", names, true);
                if(Inventory.ItemCount() > 0) eb.AddField("Item Count", count, true);
            } else if (Page == ITEM_PAGE) {
                // if it's an item page
                Item i = GetItemAtCursor();

                eb.WithTitle(i.Name);
                eb.WithColor(Item.GetColorFromRarity(i.Rarity));
                eb.WithThumbnailUrl(i.GetImageUrl());

                eb.AddField("Number Owned", $"{Inventory.ItemCount(i.Id)}", true);
                eb.AddField("Sell Price", $"{i.GetSellPrice()} Kamtrokens", true);

                string menu = "";

                if (i is IUsable) {
                    if (ItCursor == 0) {
                        // use item
                        menu += $"{CustomEmotes.CursorAnimated}Use Item\n";
                        menu += $"{CustomEmotes.CursorBlankSpace}Sell Item";
                    } else {
                        menu += $"{CustomEmotes.CursorBlankSpace}Use Item\n";
                        menu += $"{CustomEmotes.CursorAnimated}Sell Item";
                    }
                } else {
                    menu = $"{CustomEmotes.CursorAnimated}Sell Item";
                }

                eb.AddField("Actions", menu);

                eb.WithDescription(i.Description);
            } else if (Page == SELL_PAGE) {
                eb.WithTitle($"Selling {GetItemAtCursor().Name}");
                eb.AddField("Number of Items", $"{SellCount}", true);
                eb.AddField("Value", $"{SellCount * GetItemAtCursor().GetSellPrice()} Kamtrokens", true);
            } else {
                eb.WithColor(BotUtils.Red);
                eb.AddField("ERROR", $"Something went wrong! Please ping Arcy and Carbon \nPage: {Page}");
            }

            if (InvalidSellCount) {
                InvalidSellCount = false;
                // display the message
                eb.AddField("Error", "Something went wrong with the number of items you selected to sell! Please ping arcy and carbon!");
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch (option.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:
                    if (Page == HOME_PAGE) {
                        List<uint> Items = Inventory.Items.Keys.ToList();
                        Items.Sort();

                        Page = ITEM_PAGE;

                        SelectedItem = Items[Cursor];
                        await UpdateEmbed();
                    } else if (Page == ITEM_PAGE) {
                        if (ItCursor == 0) {
                            if (GetItemAtCursor() is IUsable) {
                                // if the item can be used, the cursor is on the use option
                                // so use the item
                                ConfirmEmbed ce = new ConfirmEmbed(Context, "Are you sure you want to use the item?", UseItem);
                                await ce.Display();
                            } else {
                                await SwitchToSellPage();
                            }
                        } else {
                            await SwitchToSellPage();
                        }
                    } else {
                        // on sell page, so process sell.
                        if (SellCount <= Inventory.ItemCount(GetItemAtCursor()) && SellCount > 0) {
                            // if the user has enough items to sell
                            ConfirmEmbed ce = new ConfirmEmbed(Context, $"Are you sure you want to sell {SellCount} {GetItemAtCursor().Name}{(SellCount == 1 ? "":"s")} for {SellCount * GetItemAtCursor().GetSellPrice()} Kamtrokens?", SellItem);
                            await ce.Display();
                        } else {
                            // if the user has somehow selected more items than they have, correct the number and do nothing.
                            SellCount = 0;
                            InvalidSellCount = true;
                            await UpdateEmbed();
                        }
                    }
                    break;

                case ReactionHandler.BACK_STR:
                    if (Page > HOME_PAGE) {
                        Page--;
                        ItCursor = 0;

                        if (Cursor >= Inventory.Items.Count()) Cursor = Inventory.Items.Count();

                        if (Page == -1) {
                            List<uint> Items = Inventory.Items.Keys.ToList();
                            Items.Sort();
                            SelectedItem = Items[Cursor];
                        }

                        await UpdateEmbed();
                    } else {
                        Page = HOME_PAGE;
                        await UpdateEmbed();
                    }
                    break;

                case ReactionHandler.UP_STR:
                    await CursorUp();
                    break;

                case ReactionHandler.DOWN_STR:
                    await CursorDown();
                    break;
            }
        }

        private async Task CursorUp() {
            if (Page == HOME_PAGE) {
                Cursor--;

                if (Cursor < 0) Cursor = Inventory.Items.Count - 1;

                List<uint> Items = Inventory.Items.Keys.ToList();
                Items.Sort();
                SelectedItem = Items[Cursor];
            } else if(Page == ITEM_PAGE) {
                if (GetItemAtCursor() is IUsable) {
                    ItCursor--;

                    if (ItCursor < 0) ItCursor = MAX_ITEM_OPTIONS - 1;
                }
            } else {
                SellCount++;
                
                if(SellCount > Inventory.ItemCount(SelectedItem)) {
                    SellCount = 1;
                }
            }

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (Page == HOME_PAGE) {
                Cursor++;

                if (Cursor >= Inventory.Items.Count) Cursor = 0;

                List<uint> Items = Inventory.Items.Keys.ToList();
                Items.Sort();
                SelectedItem = Items[Cursor];
            } else if (Page == ITEM_PAGE) {
                if (GetItemAtCursor() is IUsable) {
                    ItCursor++;

                    if (ItCursor >= MAX_ITEM_OPTIONS) ItCursor = 0;
                } else {
                    ItCursor = 0;
                }
            } else {
                SellCount--;

                if (SellCount < 1) {
                    SellCount = Inventory.ItemCount(SelectedItem);
                }
            }

            await UpdateEmbed();
        }

        private string MakeBold(string s, int i) {
            if (Cursor == i) {
                return $"**{s}**";
            }

            return s;
        }

        private Item GetItemAtCursor() {
            if (SelectedItem == null) return null;

            return ItemManager.GetItem(SelectedItem.Value);
        }

        private async Task SwitchToSellPage() {
            Page = SELL_PAGE;
            SellCount = 0;
            await UpdateEmbed();
        }

        private async Task UseItem(bool use) {
            if (use) {
                // use the item
                await (GetItemAtCursor() as IUsable).Use(User);

                if(Inventory.ItemCount(GetItemAtCursor()) == 0) {
                    Page = HOME_PAGE;
                }

                await UpdateEmbed();
            }
        }

        private async Task SellItem(bool sell) {
            if (sell) {
                ShopManager.SellItem(User.Id, SelectedItem.Value, SellCount);

                await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"You successfully sold {SellCount} items for a total of {SellCount * GetItemAtCursor().GetSellPrice()} Kamtrokens"));
                SellCount = 0;

                if (SellCount == Inventory.ItemCount(GetItemAtCursor().Id)) {
                    // if the user's not going to have any more of the item, take them back to the inventory page
                    Cursor = 0;
                    ItCursor = 0;
                    Page = HOME_PAGE;
                    SelectedItem = null;
                } else {
                    Page = ITEM_PAGE;
                }

                await UpdateEmbed();
            }
        }
    }
}
