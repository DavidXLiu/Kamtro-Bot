using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;

using Newtonsoft.Json;

using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Managers
{
    public class UserDataManager
    {
        /// <summary>
        /// This Dictionary holds data for all of the users.
        /// The key is the user's Discord ID (unsigned long, unchangable)
        /// The value is a node with the user's basic data.
        /// So no Inventory data.
        /// 
        /// -C
        /// </summary>
        public static Dictionary<ulong, UserDataNode> UserData;

        public UserDataManager() {
            UserData = LoadUserData();
            
        }

        /// <summary>
        /// Adds a user to the Database.
        /// </summary>
        /// <remarks>
        /// This method saves every time because there aren't going to be too many new users past a certain point
        /// </remarks>
        /// <param name="user">The User to add</param>
        /// <returns>The data node that was added</returns>
        public static UserDataNode AddUser(SocketUser user) {
            UserDataNode node = new UserDataNode($"{user.Username}#{user.Discriminator}");
            UserData.Add(user.Id, node);
            SaveUserData();
            return node;
        }

        /// <summary>
        /// This feature is not yet implemented, and is currently just a placeholder.
        /// This Dictionary hold data on the users inventories.
        /// The key is the user's Discord ID (unsigned long, unchangable)
        /// The value is an object with the user's inventory in it.
        /// 
        /// -C
        /// </summary>
        // public Dictionary<ulong, UserInventoryNode> UserInventories;
        
        /// <summary>
        /// Saves the user data to it's file.
        /// </summary>
        public static void SaveUserData() {
            BotUtils.WriteToJson(UserData, DataFileNames.UserDataFile);
        }


        /// <summary>
        /// This method Deserializes the user data from the file and loads it into memory
        /// </summary>
        /// <returns>A dict with the user data in it</returns>
        public Dictionary<ulong, UserDataNode> LoadUserData() {
            string data = FileManager.ReadFullFile(DataFileNames.UserDataFile);
            return JsonConvert.DeserializeObject<Dictionary<ulong, UserDataNode>>(data) ?? new Dictionary<ulong, UserDataNode>();
        }

        /// <summary>
        /// Adds to the score of the specified user.
        /// -C
        /// </summary>
        /// <param name="user">The user who will have their score added to</param>
        /// <param name="score">The score that will be added to the user</param>
        public void AddScore(SocketGuildUser user, int score) {
            if(!UserData.ContainsKey(user.Id)) {
                // It's a user that doesn't have an entry  -C
                AddUser(user);
            }

            UserData[user.Id].Score += score;  // Add to the score

            SaveUserData();  // Save the updated data.
        }
    }
}
