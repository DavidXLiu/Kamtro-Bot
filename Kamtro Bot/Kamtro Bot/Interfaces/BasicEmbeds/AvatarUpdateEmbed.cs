using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class AvatarUpdateEmbed : KamtroEmbedBase
    {
        SocketGuildUser User;
        private string BefUrl, AftUrl;
        private DateTimeOffset MessageTimestamp;

        public AvatarUpdateEmbed(SocketGuildUser before, SocketGuildUser after) {
            User = after;
            BefUrl = before.GetAvatarUrl();
            AftUrl = after.GetAvatarUrl();
            MessageTimestamp = DateTimeOffset.Now;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle(User.Username + "#" + User.Discriminator);
            eb.WithDescription($"{User.Mention} updated their avatar");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(BefUrl);

            eb.AddField("New Avatar", BotUtils.ZeroSpace);

            eb.WithImageUrl(AftUrl);

            eb.WithTimestamp(MessageTimestamp);

            return eb.Build();
        }
    }
}
