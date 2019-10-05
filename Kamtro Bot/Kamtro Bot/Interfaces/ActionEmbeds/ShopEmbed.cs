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
    public class ShopEmbed : ActionEmbed
    {
        private const int SHOP_SLOTS = 5;

        private const string GoBack = "⏫";

        // Page key:
        // -1 means home page
        // any other number means the page for the item at that slot.
        private int Page = -1;
        private int Cursor = 0;
        private int LastItemCount = 0;  // to prevent spam
        private int ItemCount = 0;

        private UserDataNode Customer;

        private bool ShowBadNumberWarn = false; // wether or not to show the message telling the user to enter a valid number of items.

        public ShopEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            Customer = UserDataManager.GetUserData(BotUtils.GetGUser(ctx));

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, new MenuOptionNode(GoBack, "Back"));
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Shop");

            if(Page == -1) {
                eb.WithColor(BotUtils.Kamtro);

                string s = "";

                for(int i = 0; i < ShopManager.Shop.Count; i++) {
                    ShopNode sn = ShopManager.Shop[i];

                    s += $"{(Cursor == i ? CustomEmotes.CursorAnimated:CustomEmotes.CursorBlankSpace)} {ItemManager.GetItem(sn.ItemID).Name} [{sn.Price} K]\n";
                }

                if(ShopManager.Shop.Count == 0) {
                    s = "Shop Unavailable.";
                }

                eb.AddField("Daily Listings", s);

                eb.AddField("Current Balance", $"{Customer.Money}");

                eb.WithDescription("Select an item to purchase, prices are listed next to the item name");
            } else {
                if(Page > ShopManager.Shop.Count) {
                    Page = 0;
                }

                ShopNode sn = ShopManager.Shop[Page];

                eb.WithColor(Item.GetColorFromRarity(ItemManager.GetItem(sn.ItemID).Rarity));

                eb.AddField(ItemManager.GetItem(sn.ItemID).Name, ItemManager.GetItem(sn.ItemID).Description);

                eb.AddField("Price", $"{sn.Price} Kamtrokens");

                eb.AddField("Item Count", $"{ItemCount}");
                eb.AddField("Total Cost", $"{sn.Price * ItemCount}");
                eb.AddField("Current Balance", $"{Customer.Money}");

                if (ShowBadNumberWarn) {
                    eb.AddField("Error", "Please enter a valid number of items greater than 0");
                }

                eb.WithDescription("Use the arrows to increase or decrease the number of items you want to purchase, then use the checkmark to buy them.");
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:
                    if(Page == -1) {
                        if (ShopManager.Shop.Count == 0) return;
                        Page = Cursor;
                        await UpdateEmbed();
                    } else {
                        // buy the item
                        if(ItemCount <= 0) {
                            ShowBadNumberWarn = true;
                            await UpdateEmbed();
                        } else {
                            int total = ShopManager.Shop[Page].Price * ItemCount;

                            if(total <= 0) {
                                // if the total is negative or zero, something went wrong.
                                ItemCount = 0;
                                ShowBadNumberWarn = true;
                                await UpdateEmbed();
                                return;
                            }

                            ShowBadNumberWarn = false;

                            if (total > Customer.Money && LastItemCount != ItemCount) {
                                // To prevent easy spam
                                int missing = total - Customer.Money;

                                await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"You don't have enough Kamtrokens for that! (Missing {missing} kamtroken{(missing == 1 ? "":"s")})"));
                                return;
                            }

                            // If they got this far they have money and have entered a valid number of items
                            bool bought = ShopManager.BuyItem(Context.User.Id, Page, ItemCount);


                            if(bought) {
                                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Thank you for your purchase!"));
                            } else {
                                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Something went wrong with the purchase, you have been refunded."));
                            }
                        }
                    }
                    break;

                case ReactionHandler.UP_STR:
                    await CursorDown();
                    break;

                case ReactionHandler.DOWN_STR:
                    await CursorUp();
                    break;

                case GoBack:
                    if (Page != -1) Page = -1;
                    LastItemCount = ItemCount;
                    ItemCount = 0;
                    Cursor = 0;
                    await UpdateEmbed();
                    break;
            }
        }

        private async Task CursorUp() {
            if (Page == -1) { 
                Cursor++;

                if (Cursor >= SHOP_SLOTS - 1) Cursor = 0;
            } else {
                if (ItemCount > 0) {
                    LastItemCount = ItemCount;
                    ItemCount--;
                } else {
                    LastItemCount = ItemCount;
                    ItemCount = 0;
                    return;
                }
            }

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (Page == -1) {
                Cursor--;
                if (Cursor < 0) Cursor = SHOP_SLOTS - 1;
            } else {
                LastItemCount = ItemCount;
                ItemCount++;
            }

            await UpdateEmbed();
        }
    }
}
