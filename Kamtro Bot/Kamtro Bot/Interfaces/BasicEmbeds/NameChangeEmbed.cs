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
        private SocketGuildUser User;
        private bool Nickname;
        private string Bef, Aft;
        private string Url;
        private DateTimeOffset MessageTimestamp;

        public NameChangeEmbed(SocketGuildUser before, SocketGuildUser after, bool nickname = false) {
            User = after;
            Nickname = nickname;
            Url = after.GetAvatarUrl();

            if (nickname) {
                Bef = before.Nickname;
                Aft = after.Nickname;
            } else {
                Bef = BotUtils.GetFullUsername(before);
                Aft = BotUtils.GetFullUsername(after);
            }

            MessageTimestamp = DateTimeOffset.Now;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle(User.Username + "#" + User.Discriminator);

            if (Nickname) {
                eb.WithDescription("Nickname Change");
            } else {
                eb.WithDescription("Username change");
            }

            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(Url);

            // Check if nickname was just made or removed
            if (!Nickname)
                eb.AddField($"{Bef} -----> {Aft}", BotUtils.ZeroSpace);
            else if (Nickname && Bef == null)
                eb.AddField($"{User.Username} set their nickname to {Aft}", BotUtils.ZeroSpace);
            else if (Nickname && Aft == null)
                eb.AddField($"{User.Username} removed their nickname.", BotUtils.ZeroSpace);
            else
                eb.AddField($"{Bef} -----> {Aft}", BotUtils.ZeroSpace);

            eb.WithTimestamp(MessageTimestamp);

            return eb.Build();
        }
    }
}
