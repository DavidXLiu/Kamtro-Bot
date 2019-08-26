using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class AchievementNotifyEmbed : KamtroEmbedBase
    {
        private TitleNode Title;
        private SocketGuildUser User;

        public AchievementNotifyEmbed(SocketGuildUser user, TitleNode title) {
            Title = title;
            User = user;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Achievement Get!");
            eb.WithColor(Title.GetColor());

            eb.AddField("Title", Title.Name);
            eb.AddField("Description", Title.Description);
            eb.AddField("Kamtroken Reward", Title.KamtrokenReward, true);
            eb.AddField("Temporary Rep Point Reward", Title.TempRepReward, true);

            if(Title.PermRepReward > 0) {
                UserDataNode u = UserDataManager.GetUserData(User);
                eb.AddField("Other Rewards:", $"This title has granted you +{Title.PermRepReward} to your weekly max reputation balance! This means that at the end of each week, the number of reputation points you have left to give will be set to {u.MaxReputation} if it isn't already above thta number");
            }

            return eb.Build();
        }
    }
}
