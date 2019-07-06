using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class StrikeNotifyEmbed : KamtroEmbedBase
    {
        private string Reason;
        private int Number;

        public StrikeNotifyEmbed(string reason, int number = 0) {
            Reason = reason;
            Number = number;
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("You have recieved a strike.");
            builder.WithColor(BotUtils.Orange);

            builder.AddField("Reason:", Reason);

            if(Number > 0) builder.AddField("​", $"This is your {GetStrikeNumber()} strike."); // The first set of quotes here has a zero-width space (You can't see it, but it's there)  -C

            return builder.Build();
        }

        private string GetStrikeNumber() {
            switch(Number) {
                case 1:
                    return "first";

                case 2:
                    return "second";

                case 3:
                    return "third";
            }

            return Number.ToString();
        }
    }
}
