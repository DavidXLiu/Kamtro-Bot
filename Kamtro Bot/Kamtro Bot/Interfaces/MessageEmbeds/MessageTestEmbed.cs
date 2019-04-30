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
using Discord.Commands;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class MessageTestEmbed : MessageEmbed
    {
        [InputField("TEstig", 1, 1)]
        public string Test;

        [InputField("SecondTezt", 1, 2)]
        public string SecondTest;

        public MessageTestEmbed(SocketCommandContext ctx) {
            AddMenuOptions(ReactionHandler.CHECK);
            Context = ctx;
            CommandCaller = ctx.User as SocketGuildUser;
            RegisterMenuFields();
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Testing");
            builder.WithColor(new Color(100, 210, 222));

            AddEmbedFields(builder);

            return builder.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            if(option.Emote.ToString() == ReactionHandler.CHECK_STR) {
                await Context.Channel.SendMessageAsync("MSG: " + Test);
            }
        }
    }
}
