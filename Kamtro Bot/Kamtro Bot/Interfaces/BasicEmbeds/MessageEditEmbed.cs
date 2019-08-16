using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class MessageEditEmbed : KamtroEmbedBase
    {
        public SocketUser User;
        public string Channel;
        public string Before, After;
        private DateTimeOffset MessageTimestamp;

        public MessageEditEmbed(SocketMessage b, SocketMessage a) {
            Before = b.Content;
            After = a.Content;
            Channel = (ServerData.Server.GetChannel(b.Channel.Id) as SocketTextChannel).Mention;
            MessageTimestamp = (DateTimeOffset)a.EditedTimestamp;

            User = a.Author;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle(User.Username + "#" + User.Discriminator);
            eb.WithDescription($"Message Edited by {User.Mention} in {Channel}");
            eb.WithColor(BotUtils.Yellow);
            eb.WithThumbnailUrl(User.GetAvatarUrl());

            eb.AddField("Old Message", Before);

            eb.AddField("New Message", After);

            eb.Timestamp = MessageTimestamp;

            return eb.Build();
        }
    }
}
