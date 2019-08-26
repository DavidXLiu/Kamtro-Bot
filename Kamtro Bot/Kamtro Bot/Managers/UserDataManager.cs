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
        public static UserDataNode AddUser(SocketGuildUser user) {
            UserDataNode node = new UserDataNode(user.Username, user.Nickname ?? "");
            UserData.Add(user.Id, node);
            SaveUserData();
            return node;
        }

        public static UserDataNode AddUser(ulong id, string username, string nickname = null) {
            UserDataNode node = new UserDataNode(username, nickname ?? "");
            UserData.Add(id, node);
            SaveUserData();
            return node;
        }

        public static UserDataNode GetUserData(SocketGuildUser user) {
            AddUserIfNotExists(user);

            return UserData[user.Id];
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
            if ((message.Author as SocketGuildUser) == null || message.Channel.Id == Program.Settings.BotChannelID) return;  // only count server messages

            bool userAdded = AddUserIfNotExists(message.Author as SocketGuildUser);  // if the user does not have an entry, add it.
            // Add score
            // x = user's consecutive messages
            int x = GeneralHandler.ConsMessages[message.Channel.Id];

            int score;

            if(!UserData.ContainsKey(message.Author.Id)) {
                UserData.Add(message.Author.Id, new UserDataNode(BotUtils.GetFullUsername(message.Author)));
            }
            
            if(UserData[message.Author.Id].WeeklyScore > SCORE_NERF) {
                score = Math.Max(0, 3 - x);  // If the user has a high enough score, nerf the gain rate
            } else {
                score = Math.Max(1, 6 - x);  // give the user a score of 5 if their message comes after another users, else give one less point for each consecutive message down to 1 point per message
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
        /// Gives one reputation point to a user
        /// </summary>
        /// <param name="from">The user giving the reputation point</param>
        /// <param name="to">The user recieving the reputation point</param>
        /// <returns>True if the donor can give a rep point, false otherwise.</returns>
        public static bool AddRep(SocketGuildUser from, SocketGuildUser to) {
            AddUserIfNotExists(from);
            AddUserIfNotExists(to);

            if (UserData[from.Id].ReputationToGive <= 0) {
                return false;
            }

            UserData[from.Id].ReputationToGive--;
            UserData[to.Id].Reputation++;

            GetUserData(from).RepGiven++;

            SaveUserData();

            return true;
        }

        /// <summary>
        /// Tests to see if the user can give reputation
        /// </summary>
        /// <param name="user">The user to test</param>
        /// <returns>True if the user can give rep, false otherwise</returns>
        public static bool CanAddRep(SocketGuildUser user) {
            AddUserIfNotExists(user);

            return UserData[user.Id].ReputationToGive > 0;
        }

        /// <summary>
        /// Adds a user to the dict if it doesn't exist
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <returns>True if the user needed to be added, false otherwise.</returns>
        /// -C
        public static bool AddUserIfNotExists(SocketGuildUser user) {
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
        
        public static void ResetWeekly() {
            foreach(ulong key in UserData.Keys) {
                UserData[key].WeeklyScore = 0;
            }

            KLog.Info("Reset weekly message scores");
        }

        public static void ResetRep() {
            foreach (ulong key in UserData.Keys) {
                UserData[key].ReputationToGive = Math.Max(UserData[key].MaxReputation, UserData[key].ReputationToGive);
            }

            SaveUserData();

            KLog.Info("Reset giveable reputation stats");
        }
        #endregion
    }
}
