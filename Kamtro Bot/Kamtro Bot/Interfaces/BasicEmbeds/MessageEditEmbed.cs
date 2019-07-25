using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class MessageEditEmbed : KamtroEmbedBase
    {
        public SocketUser User;
        public string Before, After;

        public MessageEditEmbed(SocketMessage b, SocketMessage a) {
            Before = b.Content;
            After = a.Content;

            User = a.Author;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Message Edited by {BotUtils.GetFullUsername(User)}");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(User.GetAvatarUrl());

            eb.AddField("Old Message", Before);

            eb.AddField("New Message", After);

            return eb.Build();
        }
    }
}
