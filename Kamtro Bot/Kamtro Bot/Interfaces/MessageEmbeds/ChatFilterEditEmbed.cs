using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class ChatFilterEditEmbed : MessageEmbed
    {
        public ChatFilterEditEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            AddMenuOptions(new MenuOptionNode(ReactionHandler.SELECT_STR, "New Filter"), new MenuOptionNode(ReactionHandler.DECLINE_STR, "Remove"));
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            throw new NotImplementedException();
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Chat Blacklist");
            eb.WithColor(BotUtils.Grey);

            switch(PageNum) {
                case 1:
                    // main page
                    string s = "";

                    
                    break;
            }

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
