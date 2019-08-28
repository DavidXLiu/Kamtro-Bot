using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class CreditsEmbed : KamtroEmbedBase
    {
        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Kamtro Bot Credits");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(Program.Client.CurrentUser.GetAvatarUrl());

            eb.AddField("Version", Program.Version);

            eb.AddField("Developers", "<@118892308086128641>\n<@201419965390258177>", true);
            eb.AddField("Testers", "<@132994961481138177>\n<@197126718400626689>\n<@111288483330478080>\n<@443551186658918409>\n<@461991782364741666>\n<@185220511544901633>", true);

            eb.AddField("Thank You", "To <@91582022379655168> and <@101358677050556416> for their support in the project, and to you for using our bot!");

            return eb.Build();
        }
    }
}
