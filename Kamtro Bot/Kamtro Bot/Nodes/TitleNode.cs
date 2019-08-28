using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is the object for a title.
    /// -C
    /// </summary>
    public class TitleNode
    {
        /// <summary>
        /// Title Difficulty (For the display color)
        /// </summary>
        public enum DifficultyLevel
        {
            EASY,
            SECRET_EASY,
            MEDIUM,
            SECRET_MEDIUM,
            HARD,
            SECRET_HARD,
            VERY_HARD,
            GOD,  // This one's just for kamtro god
        }

        public string Name;  // The name of the title -C
        public string Description; // Description of the title
        public int PermRepReward; // On achieving, increaces the user's 
        public int TempRepReward;
        public int KamtrokenReward;
        public bool Secret;
        public DifficultyLevel Difficulty;

        /// <summary>
        /// This is the constructor for a TitleNode.
        /// This creates a new Title, and registers it.
        /// </summary>
        /// <param name="title">The Name of the Title</param>
        /// <param name="desc">A brief decription of the title to display</param>
        /// <param name="kr">Kamtroken Reward</param>
        /// <param name="prr">Permanent max rep increace reward</param>
        /// <param name="trr">Temp rep points given as reward</param>
        /// <param name="secret">If the title is secret, and not on the title list</param>
        public TitleNode(string title, string desc, int prr, int trr, int kr, DifficultyLevel difficulty, bool secret = false) {
            Name = title;  // set the name
            Description = desc;
            PermRepReward = prr;
            TempRepReward = trr;
            KamtrokenReward = kr;
            Secret = secret;
            Difficulty = difficulty;
        }

        public void OnComplete(SocketGuildUser user) {
            UserDataNode data = UserDataManager.GetUserData(user);
            data.Money += KamtrokenReward;
            data.MaxReputation += PermRepReward;
            data.ReputationToGive += TempRepReward;
        }

        /// <summary>
        /// Gets the color for the display embed
        /// </summary>
        /// <returns>The color</returns>
        public Color GetColor() {
            switch(Difficulty) {
                case DifficultyLevel.EASY:
                    return BotUtils.Blue;

                case DifficultyLevel.SECRET_EASY:
                    return BotUtils.Purple;

                case DifficultyLevel.MEDIUM:
                    return BotUtils.Green;

                case DifficultyLevel.SECRET_MEDIUM:
                    return BotUtils.PurpleMagenta;

                case DifficultyLevel.HARD:
                    return BotUtils.Orange;

                case DifficultyLevel.SECRET_HARD:
                    return BotUtils.BrightMagenta;

                case DifficultyLevel.VERY_HARD:
                    return BotUtils.Red;

                case DifficultyLevel.GOD:
                    return BotUtils.Yellow;

                default:
                    return BotUtils.White;
            }
        }

        public bool DifficultyHigherThan(TitleNode other) {
            return (int)Difficulty > (int)other.Difficulty;
        }

        public bool DifficultyLowerThan(TitleNode other) {
            return false;
        }
    }
}
