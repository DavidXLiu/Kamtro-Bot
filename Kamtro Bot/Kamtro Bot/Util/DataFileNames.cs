using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot 
{
    public static class DataFileNames
    {
        /// <summary>
        /// This is an array of all the directories for the files the bot needs
        /// If you add a folder name to this list then the bot will create the dir on startup
        /// Make sure any necessary folders are in this list.
        /// </summary>
        public static readonly string[] Folders = {"Config", "User Data", "Admin", "Help", "Change Log"};

        // Config Files  -C
        public const string GeneralConfigFile = @"Config\settings.json";
        public const string TitleListFile = @"Config\Titles.json";
        public const string ItemMapFile = @"Config\Items.json";
        public const string RoleSelectMapFile = @"Config\RoleMap.json";
        public const string ShopDataFile = @"Config\ShopData.json";

        // User Data  -C
        public const string UserDataFile = @"User Data\UserData.json";
        public const string UserSettingsFile = @"User Data\UserSettings.json";
        public const string UserInventoriesFile = @"User Data\UserInventories.json";
        public const string UserRemindersFile = @"User Data\Reminders.json";

        // Ban List
        public const string AutoBanFile = @"Admin\AutoBans.json";

        // Change Log
        public const string ChangeLogOrderFile = @"Change Log\ChangeLogOrder.txt";

        public const string LastDateFile = @"User Data\LastResets.json";
    }
}