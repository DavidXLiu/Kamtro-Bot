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
        public int Money;  // Kamtrokens  -C
        public int CurrentTitle;  // The id of user's selected title.  -C
        public List<int> Titles;  // A list of title ids the user has  -C
        public int Strikes;  // The number of strikes a user has  -C
        public bool Nsfw;  // if the user has access to NSFW

        public UserDataNode(string username, int score = 0, int rep = 0, int strikes = 0, bool nsfw = true) {
            Username = username;

            Score = score;
            Reputation = rep;
            Strikes = strikes;
            Nsfw = nsfw;
        }
    }
}
