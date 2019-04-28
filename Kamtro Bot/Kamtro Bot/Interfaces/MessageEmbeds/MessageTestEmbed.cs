using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kamtro_Bot.Util;
using Kamtro_Bot.Nodes;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class MessageTestEmbed : MessageEmbed
    {
        [InputField("TEstig", 1, 1)]
        public string Test;

        public MessageTestEmbed() {
            AddMenuOptions(ReactionHandler.CHECK);
            RegisterMenuFields();
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Testing");
            builder.WithColor(new Color(100, 210, 222));

            AddEmbedFields(builder);

            return builder.Build();
        }

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
