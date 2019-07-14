using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is the node that will contain a user's stats
    /// -C
    /// </summary>
    public class UserDataNode
    {
        public string Username;
        public int Score;  // Message score  -C
        public int Reputation;  // Reputation points  -C
        public int ReputationToGive; // Rep they have left to give to others
        public int Money;  // Kamtrokens  -C
        public int CurrentTitle;  // The id of user's selected title.  -C
        public List<int> Titles;  // A list of title ids the user has  -C
        public int Strikes;  // The number of strikes a user has. (This might end up getting removed, since the strike system is already in place) -C
        public bool Nsfw;  // if the user has access to NSFW

        public UserInventoryNode Inventory;

        public UserDataNode(string username, int score = 0, int rep = 0, int repg = 3, int strikes = 0, bool nsfw = true) {
            Username = username;

            Score = score;
            Reputation = rep;
            ReputationToGive = repg;
            Strikes = strikes;
            Nsfw = nsfw;
        }
    }
}
