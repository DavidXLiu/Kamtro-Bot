using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class MessageDeleteEmbed : KamtroEmbedBase
    {
        SocketUser Author;
        string Text;

        public MessageDeleteEmbed(SocketMessage msg) {
            Text = msg.Content;
            Author = msg.Author;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Message from {BotUtils.GetFullUsername(Author)} deleted");
            eb.WithColor(BotUtils.Orange);
            eb.WithThumbnailUrl(Author.GetAvatarUrl());

            eb.AddField("Message Content", Text);

            return eb.Build();
        }
    }
}
