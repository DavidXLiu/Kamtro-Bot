using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;

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

        private SocketGuildUser User;

        public CraftingEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.BACK);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            if(Page == HOME_PAGE) {
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

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
