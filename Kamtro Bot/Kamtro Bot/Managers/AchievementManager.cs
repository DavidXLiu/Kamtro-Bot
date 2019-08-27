﻿using Discord.WebSocket;
using Kamtro_Bot.Interfaces.BasicEmbeds;
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

            //TODO: Give rep titles here
        }
        #endregion
        #region Util
        public static async Task AddTitle(SocketGuildUser user, int titleid) {
            if(!NodeMap.ContainsKey(titleid)) {
                KLog.Error($"Attempted to give user {BotUtils.GetFullUsername(user)} invalid title ID #{titleid}");
                return;
            }

            TitleNode node = NodeMap[titleid];

            if(node == null) {
                KLog.Error($"Attempted to give user {BotUtils.GetFullUsername(user)} null title with ID #{titleid}");
                return;
            }

            UserDataNode u = UserDataManager.GetUserData(user);

            u.Titles.Add(titleid);
            node.OnComplete(user);
            KLog.Important("");
            UserDataManager.SaveUserData();

            await AnnounceAchievement(user, node);
        }

        private static async Task AnnounceAchievement(SocketGuildUser user, TitleNode title) {
            AchievementNotifyEmbed ane = new AchievementNotifyEmbed(user, title);

            if((int)title.Difficulty > (int)TitleNode.DifficultyLevel.HARD && !(title.Difficulty == TitleNode.DifficultyLevel.SECRET_EASY || title.Difficulty == TitleNode.DifficultyLevel.SECRET_MEDIUM || title.Difficulty == TitleNode.DifficultyLevel.SECRET_HARD)) {
                await ane.Display(ServerData.BotChannel);
            } else {
                bool sent = await BotUtils.DMUserAsync(user, ane.GetEmbed());

                if(!sent) {
                    await ane.Display(ServerData.BotChannel); // Notify the user somehow
                }
            }
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