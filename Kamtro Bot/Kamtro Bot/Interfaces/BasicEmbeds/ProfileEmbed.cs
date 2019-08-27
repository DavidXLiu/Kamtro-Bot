using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    /// <summary>
    /// Profile embed class. For user profiles
    /// </summary>
    /// <remarks>
    /// TODO:
    /// 
    /// - Add Titles
    /// - Add Kamtrokens
    /// - Add Inventory
    /// </remarks>
    public class ProfileEmbed : KamtroEmbedBase
    {
        private UserDataNode Data;
        private SocketUser User;

        public ProfileEmbed(UserDataNode data, SocketUser user) {
            Data = data;
            User = user;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();
            
            if (!string.IsNullOrWhiteSpace(Data.Nickname))
                eb.WithTitle(Data.Nickname);
            else
                eb.WithTitle(Data.Username);

            eb.WithColor(Data.ProfileColor);
            eb.WithThumbnailUrl(User.GetAvatarUrl());

            eb.AddField("Title", "**Coming Soon!**");

            /* for hotifxes
            // eb.AddField("Weekly Activity Rating:", Data.WeeklyScore.ToString()); // TBA eventually probably  -C
            eb.AddField("Total Activity Rating", Data.Score.ToString(), true);
            eb.AddField("Kamtrokens", Data.Money, true);
            eb.AddField("Reputation Score", Data.Reputation.ToString(), true);
            eb.AddField("Max Reputation per Week", Data.MaxReputation, true);
            eb.AddField("Titles Obtained", "**Coming Soon!**", true);
            */

            eb.AddField("Total Activity Rating", Data.Score.ToString(), true);
            eb.AddField("Reputation Score", Data.Reputation.ToString(), true);
            eb.AddField("Kamtrokens", "**Coming Soon!**", true);
            eb.AddField("Max Reputation per Week", Data.MaxReputation, true);
            eb.AddField("Titles Obtained", "**Coming Soon!**", true);

            if (!string.IsNullOrWhiteSpace(Data.Quote)) eb.WithFooter(Data.Quote);

            return eb.Build();
        }
    }
}
