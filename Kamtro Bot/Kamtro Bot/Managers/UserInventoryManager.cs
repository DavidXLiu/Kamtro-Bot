using Kamtro_Bot.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public class UserInventoryManager
    {
        public static void LoadInventories() {
            string data = FileManager.ReadFullFile(DataFileNames.UserInventoriesFile);
            Dictionary<ulong, UserInventoryNode> UserInventories = JsonConvert.DeserializeObject<Dictionary<ulong, UserInventoryNode>>(data) ?? new Dictionary<ulong, UserInventoryNode>();

            foreach(ulong id in UserInventories.Keys) {
                if(UserDataManager.UserData.ContainsKey(id)) {
                    // User has a data file, things are all good
                    UserDataManager.GetUserData(BotUtils.GetGUser(id)).Inventory = UserInventories[id];
                } else {
                    // wacky things are going on, create a user profile and then load inventory
                    UserDataManager.AddUser(BotUtils.GetGUser(id));
                    UserDataManager.GetUserData(BotUtils.GetGUser(id)).Inventory = UserInventories[id];
                }
            }

            KLog.Info("Loaded Inventories.");
        }

        public static void SaveInventories() {
            Dictionary<ulong, UserInventoryNode> UserInventories = new Dictionary<ulong, UserInventoryNode>();

            foreach(ulong i in UserDataManager.UserData.Keys) {
                if (BotUtils.GetGUser(i) == null) continue;  // null checking.

                UserInventories.Add(i, UserDataManager.GetUserData(BotUtils.GetGUser(i)).Inventory ?? new UserInventoryNode());
            }

            BotUtils.WriteToJson(UserInventories, DataFileNames.UserInventoriesFile);
            KLog.Info("Saved Inventories.");
        }

        public static UserInventoryNode GetInventory(ulong userId) {
            UserDataManager.AddUserIfNotExists(BotUtils.GetGUser(userId));

            if (UserDataManager.GetUserData(BotUtils.GetGUser(userId)).Inventory == null) {
                UserDataManager.GetUserData(BotUtils.GetGUser(userId)).Inventory = new UserInventoryNode();
                SaveInventories();
            }

            return UserDataManager.GetUserData(BotUtils.GetGUser(userId)).Inventory;
        }
    }
}
