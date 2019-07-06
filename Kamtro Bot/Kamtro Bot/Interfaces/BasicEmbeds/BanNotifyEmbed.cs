using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class BanNotifyEmbed : KamtroEmbedBase
    {
        private string Reason;

        public BanNotifyEmbed(string reason) {
            Reason = reason;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Ban Notice");
            eb.WithColor(BotUtils.Red);

            eb.AddField(BotUtils.ZeroSpace, "You have been banned from Kamtro.");
            eb.AddField("Reason:", Reason);

            return eb.Build();
        }
    }
}
