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
        public static Dictionary<ulong, UserInventoryNode> UserInventories;

        public static void LoadInventories() {
            string data = FileManager.ReadFullFile(DataFileNames.UserInventoriesFile);
            Dictionary<ulong, UserInventoryNode> UserInventories = JsonConvert.DeserializeObject<Dictionary<ulong, UserInventoryNode>>(data) ?? new Dictionary<ulong, UserInventoryNode>();

            KLog.Info("Loaded Inventories.");
        }

        public static void SaveInventories() {
            Dictionary<ulong, UserInventoryNode> UserInventories = null;

            BotUtils.WriteToJson(UserInventories, DataFileNames.UserInventoriesFile);
            KLog.Info("Saved Inventories.");
        }

        public static UserInventoryNode GetInventory(ulong userId) {
            return UserDataManager.GetUserData(BotUtils.GetGUser(userId)).Inventory;
        }
    }
}
