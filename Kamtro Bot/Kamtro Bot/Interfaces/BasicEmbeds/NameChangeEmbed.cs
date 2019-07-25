using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class NameChangeEmbed : KamtroEmbedBase
    {
        private bool Nickname;
        private string Bef, Aft;
        private string Url;

        public NameChangeEmbed(SocketGuildUser before, SocketGuildUser after, bool nickname = false) {
            Nickname = nickname;
            Url = after.GetAvatarUrl();

            if (nickname) {
                Bef = before.Nickname;
                Aft = after.Nickname;
            } else {
                Bef = BotUtils.GetFullUsername(before);
                Aft = BotUtils.GetFullUsername(after);
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            if (Nickname) {
                eb.WithTitle("Nickname Change");
            } else {
                eb.WithTitle("Username change");
            }

            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(Url);
            eb.AddField($"{Bef} -----> {Aft}", BotUtils.ZeroSpace);

            return eb.Build();
        }
    }
}
