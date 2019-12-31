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
        public static Dictionary<ulong, UserSettingsNode> UserSettings;

        public const int SCORE_NERF = 1000;

        public static bool Saving = false;

        public UserDataManager() {
            UserData = LoadUserData();
            UserSettings = LoadUserSettings();
        }

        /// <summary>
        /// Adds a user to the Database.
        /// </summary>
        /// <remarks>
        /// This method saves every time because there aren't going to be too many new users past a certain point
        /// </remarks>
        /// <param name="user">The User to add</param>
        /// <returns>The data node that was added</returns>
        public static Tuple<UserDataNode, UserSettingsNode> AddUser(SocketGuildUser user) {
            if(!UserData.ContainsKey(user.Id)) {
                UserDataNode node = new UserDataNode(user.Username, user.Nickname ?? "");
                UserData.Add(user.Id, node);
                GetUserData(user).Inventory = new UserInventoryNode();
                SaveUserData();
            }

            if(!UserSettings.ContainsKey(user.Id)) {
                UserSettingsNode node = new UserSettingsNode(BotUtils.GetFullUsername(user));
                UserSettings.Add(user.Id, node);
                SaveUserSettings();
            }

            Tuple<UserDataNode, UserSettingsNode> value = new Tuple<UserDataNode, UserSettingsNode>(GetUserData(user), GetUserSettings(user));

            return value;
        }

        public static Tuple<UserDataNode, UserSettingsNode> AddUser(ulong id, string username, string nickname = null) {
            UserDataNode dNode = null;
            UserSettingsNode sNode = null;

            if (!UserData.ContainsKey(id)) { 
                dNode = new UserDataNode(username, nickname ?? "");
                UserData.Add(id, dNode);
            }

            if (!UserSettings.ContainsKey(id)) {
                sNode = new UserSettingsNode(username);
                UserSettings.Add(id, sNode);
            }

            SaveUserData();

            return new Tuple<UserDataNode, UserSettingsNode>(dNode, sNode);
        }

        public static void FullSave() {
            KLog.Info("Beginning full save...\n---------------");
            SaveUserData();
            SaveUserSettings();
            UserInventoryManager.SaveInventories();
            KLog.Info("---------------");
        }

        #region Exparimental
        /// <summary>
        /// This feature is not yet implemented, and is currently just a placeholder.
        /// This Dictionary hold data on the users inventories.
        /// The key is the user's Discord ID (unsigned long, unchangable)
        /// The value is an object with the user's inventory in it.
        /// 
        /// -C
        /// </summary>
        // public Dictionary<ulong, UserInventoryNode> UserInventories;
        #endregion
        #region User Data
        public static UserDataNode GetUserData(SocketGuildUser user) {
            if(user == null) return null;

            AddUserIfNotExists(user);

            return UserData[user.Id];
        }

        public static bool HasTitle(SocketGuildUser user, int title) {
            return AchievementManager.HasTitleUnlocked(user, title);
        }
        
        public static void EquipTitle(SocketGuildUser user, int title) {
            GetUserData(user).CurrentTitle = title;
            SaveUserData();
        }
        #endregion
        #region User Settings
        public static UserSettingsNode GetUserSettings(SocketGuildUser user) {
            AddUserIfNotExists(user);

            return UserSettings[user.Id];
        }
        #endregion
        #region Event
        public static async Task OnChannelMessage(SocketUserMessage message) {
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

            await AddScore(BotUtils.GetGUser(message.Author.Id), score);
            // End of score calculation

            if (userAdded && !BotUtils.SaveInProgress) SaveUserData();  // save the data if the user was added, but only if autosave isn't in progress.
        }
        #endregion
        #region Utility
        /// <summary>
        /// Adds a user to the dict if it doesn't exist
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <returns>True if the user needed to be added, false otherwise.</returns>
        /// -C
        public static bool AddUserIfNotExists(SocketGuildUser user) {
            bool added = false;

            if (user != null && (!UserData.ContainsKey(user.Id) || !UserSettings.ContainsKey(user.Id))) {
                AddUser(user);
                added =  true;
            }

            return added;
        }
        
        #region User Data
        /// <summary>
        /// Saves the user data to it's file.
        /// </summary>
        public static void SaveUserData() {
            if(!Saving && !BotUtils.SaveInProgress) {
                Saving = true;
                BotUtils.WriteToJson(UserData, DataFileNames.UserDataFile);
                Saving = false;
            } else {
                KLog.Warning("Tried to save user data, but couldn't becuase it was already being saved!");
            }
        }

        /// <summary>
        /// Gives one reputation point to a user
        /// </summary>
        /// <param name="from">The user giving the reputation point</param>
        /// <param name="to">The user recieving the reputation point</param>
        /// <returns>True if the donor can give a rep point, false otherwise.</returns>
        public static async Task<bool> AddRep(SocketGuildUser from, SocketGuildUser to) {
            AddUserIfNotExists(from);
            AddUserIfNotExists(to);

            if (UserData[from.Id].ReputationToGive <= 0) {
                return false;
            }

            GetUserData(from).ReputationToGive--;
            GetUserData(to).Reputation++;

            GetUserData(from).RepGiven++;

            await AchievementManager.OnRep(from, to);

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
        public static async Task AddScore(SocketGuildUser user, int score) {
            AddUserIfNotExists(user);

            UserDataNode data = GetUserData(user);
            data.Score += score;  // Add to the score
            
            if(data.KamtrokensEarned < 10) data.KamtrokenEarnProgress += score;
            
            if(data.KamtrokenEarnProgress >= 100 && data.KamtrokensEarned < 10) {
                data.KamtrokenEarnProgress -= 100;
                data.Kamtrokens++;
                data.KamtrokensEarned++;

                SaveUserData(); 
            }
                
            data.WeeklyScore += score;

            await AchievementManager.OnScore(user);

            SaveUserData();  // Save the updated data.
        }

        public static void ResetKamtrokenEarns() {
            foreach (ulong key in UserData.Keys) {
                UserData[key].KamtrokensEarned = 0;
                UserData[key].KamtrokenEarnProgress = 0;
            }

            SaveUserData();

            KLog.Info("Reset daily earned kamtrokens");
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
        #region User Settings
        public Dictionary<ulong, UserSettingsNode> LoadUserSettings() {
            string data = FileManager.ReadFullFile(DataFileNames.UserSettingsFile);
            return JsonConvert.DeserializeObject<Dictionary<ulong, UserSettingsNode>>(data) ?? new Dictionary<ulong, UserSettingsNode>();
        }

        public static void SaveUserSettings() {
            BotUtils.WriteToJson(UserSettings, DataFileNames.UserSettingsFile);
            KLog.Info("Saved user settings.");
        }
        #endregion
        #endregion
    }
}
