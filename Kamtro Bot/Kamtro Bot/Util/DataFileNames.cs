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
        public static readonly string[] Folders = {"Config", "User Data", "Admin", "Help"};

        // Config Files  -C
        public const string GeneralConfigFile = @"Config\settings.json";
        public const string TitleListFile = @"Config\Titles.json";

        // User Data  -C
        public const string UserDataFile = @"User Data\UserData.json";

        // Ban List
        public const string AutoBanFile = @"Admin\AutoBans.json";
    }
}