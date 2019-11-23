using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class DiceRollEmbed : KamtroEmbedBase
    {
        private List<int> Results;
        private int Side;

        public DiceRollEmbed(int sides, List<int> res) {
            Results = res;
            Side = sides;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Dice Roll");
            eb.WithColor(BotUtils.Kamtro);

            eb.AddField("Number of Dice", Results.Count, true);
            eb.AddField("Number of Sides", Side, true);

            string result = BotUtils.ZeroSpace;

            for(int i = 0; i < Results.Count; i++) {
                result += $"Roll {i+1}: {Results[i]}";
            }

            eb.AddField("Results", result);

            return eb.Build();
        }
    }
}
