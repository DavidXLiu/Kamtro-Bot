using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // Something important to note about the Newtonsoft JSON serializer
        // It will not serialize static variables unless specifically told to
        // This is very useful, because it means that this Dictionary won't be 
        // serialized into every title node entry
        // -C
        /// <summary>
        /// This variable stores title nodes as values, and their corresponding IDs as keys.
        /// Dictionaries serialize quite well into JSON.
        /// </summary>

        public string Name;  // The name of the title -C
        public string Description; // Description of the title
        public int PermRepReward; // On achieving, increaces the user's 
        public int TempRepReward;
        public int KamtrokenReward;
        public bool Secret;


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
        public TitleNode(string title, string desc, int prr, int trr, int kr, bool secret = false) {
            Name = title;  // set the name
            Description = desc;
            PermRepReward = prr;
            TempRepReward = trr;
            KamtrokenReward = kr;
            Secret = secret;
        }

        public void OnComplete(SocketGuildUser user) {
            UserDataNode data = UserDataManager.GetUserData(user);
            data.Money += KamtrokenReward;
            data.MaxReputation += PermRepReward;
            data.ReputationToGive += TempRepReward;
        }
    }
}
