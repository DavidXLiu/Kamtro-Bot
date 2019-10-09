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
        private SocketGuildUser User;
        private UserDataNode UserData;
        private UserInventoryNode Inventory;

        private int Cursor = 0;
        private int Page = -1;

        public InventoryEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);
            UserData = UserDataManager.GetUserData(User);
            Inventory = UserInventoryManager.GetInventory(User.Id);

            AddMenuOptions(ReactionHandler.SELECT, null, ReactionHandler.UP, ReactionHandler.DOWN);
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

                    for(int i = 0; i < Items.Count; i++) {
                        Item it = ItemManager.GetItem(Items[i]);

                        if(i == Cursor) {
                            eb.WithImageUrl(it.GetImageUrl());
                        }

                        names += $"{it.Name, -1}\n";
                        count += $"{Inventory.ItemCount(Items[i])}\n";
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

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }

        private string MakeBold(string s, int i) {
            if(Cursor == i) {
                return $"**{s}**";
            }

            return s;
        }
    }
}
