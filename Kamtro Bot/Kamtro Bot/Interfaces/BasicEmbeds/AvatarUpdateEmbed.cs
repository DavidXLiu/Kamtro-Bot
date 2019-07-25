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
        private string BefUrl, AftUrl;

        public AvatarUpdateEmbed(SocketGuildUser before, SocketGuildUser after) {
            BefUrl = before.GetAvatarUrl();
            AftUrl = after.GetAvatarUrl();
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Avatar Updated");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(BefUrl);

            eb.AddField("New Avatar", BotUtils.ZeroSpace);

            eb.WithImageUrl(AftUrl);

            return eb.Build();
        }
    }
}
