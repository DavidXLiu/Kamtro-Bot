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

            eb.AddField("Title", GetTitle());

            eb.AddField("Total Activity Rating", Data.Score.ToString(), true);
            eb.AddField("Kamtrokens", Data.Kamtrokens, true);
            eb.AddField("Reputation Score", Data.Reputation.ToString(), true);
            eb.AddField("Max Reputation per Week", Data.MaxReputation, true);
            eb.AddField("Titles Obtained", GetTitleList(), true);

            if (!string.IsNullOrWhiteSpace(Data.Quote)) eb.WithFooter(Data.Quote);

            return eb.Build();
        }

        private string GetTitle() {
            if(Data.CurrentTitle == null) {
                return "[None Selected]";
            } else {
                return $"**{AchievementManager.GetTitle(Data.CurrentTitle.Value).Name}**";
            }
        }

        private string GetTitleList() {
            if(Data.Titles.Count == 0) {
                return "No Titles Obtained.";
            }

            string titles = "";

            foreach (int id in Data.Titles) {
                titles += $"{AchievementManager.GetTitle(id).Name}, ";
            }

            return titles.TrimEnd(',', ' ');
        }
    }
}
