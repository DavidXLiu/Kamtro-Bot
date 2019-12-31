using Discord.WebSocket;
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
        #region Constants
        public static class TitleIDs
        {
            public const int KAMTRO_GOD = 0;
            public const int KAMTRO_SQUIRE = 1;  // And thus, they spake
            public const int TALKATIVE = 2;
            public const int CHATTERBOX = 3;
            public const int REGULAR = 4;
            public const int KAMTRO_VETERAN = 5;
            public const int OLIVE_GARDENER = 6;  
            public const int WARMLY_WELCOMED = 7;
            public const int TEN_REP = 8;
            public const int COOL_KID = 9;
            public const int POPULAR = 10;
            public const int MILLENIAL_MEMBER = 11;
            public const int WARM_WELCOMER = 12;
            public const int DECAREPPER = 13;
            public const int CHARITABLE = 14;
            public const int HUNDRED_REP_GIVEN = 15;
            public const int REPPER_OF_D = 16;
            public const int KAMEX_BUTT = 17;
            public const int RETRO_BUTT = 18;
            public const int MLEM = 19;
            public const int FIRST_BUY = 20;
            public const int SHOPPER = 21;
            public const int FRIDAY_EXPERT = 22;
            public const int SHOPMASTER = 23;
        }
        #endregion

        public static Dictionary<int, TitleNode> NodeMap;  // This is the map that stores the nodes and their corresponding ID values
        public static bool Loaded = false;

        #region Event
        /// <summary>
        /// Method that checks for titles related to rep. Called whenever a user reps another user.
        /// </summary>
        /// <remarks>
        /// This method has a lot of checks, they are all there without return statements for a reason.
        /// It will make sure to fill in any missing titles the user should have gotten.
        /// </remarks>
        /// <param name="from">The user doing the repping</param>
        /// <param name="to">The user who has been repped</param>
        /// <returns></returns>
        public static async Task OnRep(SocketGuildUser from, SocketGuildUser to) {
            UserDataNode f = UserDataManager.GetUserData(from);
            UserDataNode t = UserDataManager.GetUserData(to);

            // rep given
            if(f.RepGiven >= 1) {
                await AddTitle(from, TitleIDs.WARM_WELCOMER);
            }

            if (f.RepGiven >= 10) {
                await AddTitle(from, TitleIDs.DECAREPPER);
            }

            if (f.RepGiven >= 50) {
                await AddTitle(from, TitleIDs.CHARITABLE);
            }

            if (f.RepGiven >= 100) {
                await AddTitle(from, TitleIDs.HUNDRED_REP_GIVEN);
            }

            if (f.RepGiven >= 500) {
                await AddTitle(from, TitleIDs.REPPER_OF_D);
            }

            // rep recieved
            if (t.Reputation >= 1) {
                await AddTitle(to, TitleIDs.WARMLY_WELCOMED);
            }

            if (t.Reputation >= 10) {
                await AddTitle(to, TitleIDs.TEN_REP);
            }

            if (t.Reputation >= 30) {
                await AddTitle(to, TitleIDs.COOL_KID);
            }

            if (t.Reputation >= 100) {
                await AddTitle(to, TitleIDs.POPULAR);
            }

            if (t.Reputation >= 1000) {
                await AddTitle(to, TitleIDs.MILLENIAL_MEMBER);
            }
        }
        
        public static async Task OnBuy(SocketGuildUser customer) {
            int spent = UserDataManager.GetUserData(customer).KamtrokensSpent;

            if(spent > 1) {
                await AddTitle(customer, TitleIDs.FIRST_BUY);
            } else if (spent > 50) {
                await AddTitle(customer, TitleIDs.SHOPPER);
            } else if (spent > 200) {
                await AddTitle(customer, TitleIDs.FRIDAY_EXPERT);
            } else if (spent > 1000) {
                await AddTitle(customer, TitleIDs.SHOPMASTER);
            } else {
                return;
            }

            UserDataManager.SaveUserData();
        }
        
        public static async Task OnScore(SocketGuildUser user) {
            int s = UserDataManager.GetUserData(user).Score;

            // for titles
            if (s >= 1) {
                await AddTitle(user, TitleIDs.KAMTRO_SQUIRE);
            }

            if (s >= 10_000) {
                await AddTitle(user, TitleIDs.TALKATIVE);
            }

            if (s >= 50_000) {
                await AddTitle(user, TitleIDs.CHATTERBOX);
            }

            if (s >= 100_000) {
                await AddTitle(user, TitleIDs.REGULAR);
            }

            if (s >= 500_000) {
                await AddTitle(user, TitleIDs.KAMTRO_VETERAN);
            }
        }

        public static async Task OnTitle(SocketGuildUser user, int titleId) {
            if (titleId == 0) return;
            
            // Kamtro God
            bool god = false;

            foreach (int i in NodeMap.Keys) {
                if (i == 0) continue;
                if (!UserDataManager.GetUserData(user).Titles.Contains(i)) god = true;
            }

            if(god) {
                await AddTitle(user, TitleIDs.KAMTRO_GOD);
            }
        }
        #endregion
        #region Util
        public static async Task AddTitle(SocketGuildUser user, int titleid) {
            if (!NodeMap.ContainsKey(titleid)) {
                KLog.Error($"Attempted to give user {BotUtils.GetFullUsername(user)} invalid title ID #{titleid}");
                return;
            }

            TitleNode node = NodeMap[titleid];

            if(node == null) {
                KLog.Error($"Attempted to give user {BotUtils.GetFullUsername(user)} null title with ID #{titleid}");
                return;
            }

            UserDataNode u = UserDataManager.GetUserData(user);
            if (u.Titles == null) u.Titles = new List<int>();

            if (u.Titles.Contains(titleid)) return;  // Don't give duplicate titles
            u.Titles.Add(titleid);
            node.OnComplete(user);

            KLog.Important($"User {BotUtils.GetFullUsername(user)} has earned title {node.Name}");

            UserDataManager.SaveUserData();

            await AnnounceAchievement(user, node);
        }

        private static async Task AnnounceAchievement(SocketGuildUser user, TitleNode title) {
            AchievementNotifyEmbed ane = new AchievementNotifyEmbed(user, title);

            if((int)title.Difficulty > (int)TitleNode.DifficultyLevel.HARD && !(title.Difficulty == TitleNode.DifficultyLevel.SECRET_EASY || title.Difficulty == TitleNode.DifficultyLevel.SECRET_MEDIUM || title.Difficulty == TitleNode.DifficultyLevel.SECRET_HARD)) {
                await BotUtils.DMUserAsync(user, ane.GetEmbed());
                ane.Header = $"{user.GetDisplayName()} got an achievement!";
                await ane.Display(ServerData.BotChannel);
            } else {
                await BotUtils.DMUserAsync(user, ane.GetEmbed());
                // Don't notify for smaller achievements
                /*bool sent = await BotUtils.DMUserAsync(user, ane.GetEmbed());

                if(!sent) {
                    await ane.Display(ServerData.BotChannel); // Notify the user somehow
                }*/
            }
        }

        public static TitleNode GetTitle(int id) {
            return NodeMap.ContainsKey(id) ? NodeMap[id] : null;
        }

        public static bool HasTitleUnlocked(SocketGuildUser user, int title) {
            return UserDataManager.GetUserData(user).Titles.Contains(title);
        }
        
        public static int TitleCount() {
            return NodeMap.Count;
        }
        
        public static List<Tuple<int, TitleNode>> GetTitles(bool secret = false) {
            List<Tuple<int, TitleNode>> titles = new List<Tuple<int, TitleNode>>();

            foreach (KeyValuePair<int, TitleNode> kv in NodeMap) {
                if(!kv.Value.Secret || secret) titles.Add(new Tuple<int, TitleNode>(kv.Key, kv.Value));
            }

            titles.Sort((x, y) => x.Item2.Name.CompareTo(y.Item2.Name));  // sort by title Name

            return titles;
        }
        #endregion
        #region File Interaction
        public static void SaveNodeMap() {
            BotUtils.WriteToJson(NodeMap, DataFileNames.TitleListFile);  // Save the file
        }

        public static void LoadNodeMap() {
            string json = FileManager.ReadFullFile(DataFileNames.TitleListFile);

            NodeMap = JsonConvert.DeserializeObject<Dictionary<int, TitleNode>>(json) ?? new Dictionary<int, TitleNode>();

            KLog.Info($"Title node map {(Loaded ? "Reloaded":"Loaded")}");
            Loaded = true;
        }
        #endregion
    }

     
}
