using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord.WebSocket;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is the node that will contain a user's stats
    /// </summary>
    /// <remarks>
    /// The stly guide for this class is as follows:
    /// - For any fields which have a CONSTANT default value (such as a number, or null), assign that value in the class definition, not in the constructor
    /// - For all other fields, assign in the constructor.
    /// - Any fields which are not displayed on the profile page must be in the Statistics region
    /// </remarks>
    public class UserDataNode
    {
        public string Username;
        public string Nickname;
        public int Score;  // Message score  -C
        public int WeeklyScore;
        public int Reputation;  // Reputation points  -C
        public int ReputationToGive; // Rep they have left to give to others
        public int MaxReputation = 3; // Max rep to give
        public int Kamtrokens;  // Kamtrokens  -C
        public int? CurrentTitle = null;  // The id of user's selected title.  -C
        public List<int> Titles;  // A list of title ids the user has  -C
        public uint ProfileColor = BotUtils.Kamtro.RawValue;
        public string Quote;
        public int Strikes;  // The number of strikes a user has. (This might end up getting removed, since the strike system is already in place) -C
        public bool Nsfw;  // if the user has access to NSFW
        public bool PorterSupporter; // if the user donated to porter  -C

        #region Statistics
        public int CommandsUsed = 0;  // TO REMOVE
        public int TimesCheckedProfile = 0;
        public int RepGiven = 0;
        public int KamtrokensSpent = 0;
        public int MaxCFStreak = 0;

        // Secret Things
        public int TimesLookedAtRetroButt = 0;
        public int TimesLookedAtKamexButt = 0;
        #endregion

        [JsonIgnore]
        public UserInventoryNode Inventory;

        public UserDataNode(string username, string nickname = "", int score = 0, int weeklyscore = 0, int reputation = 0, int reputationtogive = 3, int strikes = 0, bool nsfw = true, string quote = "", bool portersupporter = false) {
            Username = username;
            Nickname = nickname;

            Score = score;
            WeeklyScore = weeklyscore;
            Reputation = reputation;
            ReputationToGive = reputationtogive;
            Strikes = strikes;
            Nsfw = nsfw;
            Quote = quote;
            PorterSupporter = portersupporter;

            Titles = new List<int>();
        }

        public Color GetColor() {
            return new Color(ProfileColor);
        }
    }
}
