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
    public class InventoryEmbed : ActionEmbed
    {
        private const int MAX_ITEM_OPTIONS = 2;  // the number of things you can do with an item max (for now, sell and use)

        private SocketGuildUser User;
        private UserDataNode UserData;
        private UserInventoryNode Inventory;

        private int Cursor = 0;
        private int ItCursor = 0;  // cursor to select sell or use
        private int Page = -1;

        public InventoryEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);
            UserData = UserDataManager.GetUserData(User);
            Inventory = UserInventoryManager.GetInventory(User.Id);

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.BACK, ReactionHandler.UP, ReactionHandler.DOWN);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"{User.GetDisplayName()}'s Inventory");
            eb.WithColor(UserData.ProfileColor);


            if(Page == -1) {
                // if it's the home page
                string names = "";
                string count = "";

                if (Inventory.ItemCount() == 0) {
                    names = "You have no items!";
                } else {
                    List<uint> Items = Inventory.Items.Keys.ToList();
                    Items.Sort();

                    for(int i = 0; i < Items.Count; i++) {
                        Item it = ItemManager.GetItem(Items[i]);

                        if(i == Cursor) {
                            eb.WithImageUrl(it.GetImageUrl());
                        }

                        names += $"{(i == Cursor ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{it.Name, -1}\n";
                        count += $"{MakeBold(Inventory.ItemCount(Items[i]).ToString(), i)}\n";
                    }
                }

                eb.AddField("Items", names, true);
                eb.AddField("Item Count", count, true);
            } else {
                // if it's an item page
                Item i = ItemManager.GetItem(Inventory.Items.Keys.ToList()[Cursor]);

                eb.WithColor(Item.GetColorFromRarity(i.Rarity));
                eb.WithThumbnailUrl(i.GetImageUrl());
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:
                    if(Page == -1) {
                        // List<uint> Items = Inventory.Items.Keys.ToList();
                        // Items.Sort();

                        Page = Cursor;
                        await UpdateEmbed();
                    } else {
                        if(ItCursor == 0) {

                        }
                    }
                    break;

                case ReactionHandler.BACK_STR:
                    if(Page != -1) {
                        Page = -1;
                        ItCursor = 0;

                        if (Cursor >= Inventory.Items.Count()) Cursor = Inventory.Items.Count();

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
            if(Page == -1) {
                Cursor--;

                if (Cursor < 0) Cursor = Inventory.Items.Count();

                await UpdateEmbed();
            } else {
                List<uint> Items = Inventory.Items.Keys.ToList();
                Items.Sort();

                if(ItemManager.GetItem(Items[Page]) is IUsable) {
                    ItCursor--;

                    if (ItCursor < 0) ItCursor = MAX_ITEM_OPTIONS - 1;

                    await UpdateEmbed();
                } 
            }
        }

        private async Task CursorDown() {
            if (Page == -1) {
                Cursor++;

                if (Cursor >= Inventory.Items.Count) Cursor = 0;

                await UpdateEmbed();
            } else {
                List<uint> Items = Inventory.Items.Keys.ToList();
                Items.Sort();

                if (ItemManager.GetItem(Items[Page]) is IUsable) {
                    ItCursor++;

                    if (ItCursor >= MAX_ITEM_OPTIONS) ItCursor = 0;

                    await UpdateEmbed();
                } else {
                    ItCursor = 0;
                }
            }
        }

        private string MakeBold(string s, int i) {
            if(Cursor == i) {
                return $"**{s}**";
            }

            return s;
        }
    }
}
