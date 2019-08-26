using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class AchievementNotifyEmbed : KamtroEmbedBase
    {
        private TitleNode Title;

        public AchievementNotifyEmbed(TitleNode title) {
            Title = title;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            return eb.Build();
        }
    }
}
