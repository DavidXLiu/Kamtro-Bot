using Discord.WebSocket;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public class AchievementManager
    {
        public static Dictionary<int, TitleNode> NodeMap;  // This is the map that stores the nodes and their corresponding ID values
        public static bool Loaded = false;

        #region Event
        public static void OnRep(SocketGuildUser from, SocketGuildUser to) {
            UserDataNode f = UserDataManager.GetUserData(from);
            UserDataNode t = UserDataManager.GetUserData(to);


        }
        #endregion
        #region Util
        private static async Task AnnounceAchievement(SocketUser user, TitleNode title) {
            await BotUtils.DMUserAsync(user, msg: BotUtils.KamtroText($"Congradulations! You have earned the title {title.Name}!"));
        }

        public static TitleNode GetTitle(int id) {
            return NodeMap.ContainsKey(id) ? NodeMap[id] : null;
        }

        public static bool HasTitleUnlocked(SocketGuildUser user, int title) {
            return UserDataManager.GetUserData(user).Titles.Contains(title);
        }
        #endregion
        #region File Interaction
        public static void SaveNodeMap() {
            BotUtils.WriteToJson(NodeMap, DataFileNames.TitleListFile);  // Save the file
        }

        public static void LoadNodeMap() {
            string json = FileManager.ReadFullFile(DataFileNames.TitleListFile);

            NodeMap = JsonConvert.DeserializeObject<Dictionary<int, TitleNode>>(json);
            KLog.Info($"Title node map {(Loaded ? "Reloaded":"Loaded")}");
            Loaded = true;
        }
        #endregion
    }

     
}
