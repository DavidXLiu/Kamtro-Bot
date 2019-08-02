using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;

using Newtonsoft.Json;

using Kamtro_Bot.Nodes;
using Kamtro_Bot.Handlers;

namespace Kamtro_Bot.Managers
{
    /// <summary>
    /// User data manager.
    /// </summary>
    /// <remarks>
    /// This class is what should contain all the required utility methods for interacting with the user data.
    /// </remarks>
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

        public const int SCORE_NERF = 1000;

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
        #region Event
        public static void OnChannelMessage(SocketUserMessage message) {
            bool userAdded = AddUserIfNotExists(message.Author);  // if the user does not have an entry, add it.
            // Add score
            // x = user's consecutive messages
            int x = GeneralHandler.ConsMessages[message.Channel.Id];

            int score;

            if(!UserData.ContainsKey(message.Author.Id)) {
                UserData.Add(message.Author.Id, new UserDataNode(BotUtils.GetFullUsername(message.Author)));
            }
            
            if(UserData[message.Author.Id].WeeklyScore > SCORE_NERF) {
                score = Math.Max(0, 3 - x);
            } else {
                score = Math.Max(1, 5 - x);
            }

            UserData[message.Author.Id].Score += score;
            UserData[message.Author.Id].WeeklyScore += score;
            // End of score calculation

            if (userAdded && !BotUtils.SaveInProgress) SaveUserData();  // save the data if the user was added, but only if autosave isn't in progress.
        }
        #endregion
        #region Utility
        /// <summary>
        /// Saves the user data to it's file.
        /// </summary>
        public static void SaveUserData() {
            BotUtils.WriteToJson(UserData, DataFileNames.UserDataFile);
        }

        /// <summary>
        /// Adds a user to the dict if it doesn't exist
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <returns>True if the user needed to be added, false otherwise.</returns>
        /// -C
        public static bool AddUserIfNotExists(SocketUser user) {
            if(!UserData.ContainsKey(user.Id)) {
                AddUser(user);
                return true;
            }

            return false;
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
        /// Sets the NSFW blacklist status of the user.
        /// </summary>
        /// -C
        /// <param name="user">The user to edit</param>
        /// <param name="blacklisted">True if they are blacklisted from NSFW, false otherwise</param>
        public static void SetNSFW(SocketGuildUser user, bool blacklisted) {
            AddUserIfNotExists(user);

            UserData[user.Id].Nsfw = blacklisted;
            SaveUserData();
        }

        /// <summary>
        /// Adds to the score of the specified user.
        /// -C
        /// </summary>
        /// <param name="user">The user who will have their score added to</param>
        /// <param name="score">The score that will be added to the user</param>
        public static void AddScore(SocketGuildUser user, int score) {
            AddUserIfNotExists(user);

            UserData[user.Id].Score += score;  // Add to the score

            SaveUserData();  // Save the updated data.
        }

        #endregion
    }
}
