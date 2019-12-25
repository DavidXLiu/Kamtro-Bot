using Kamtro_Bot.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public static class ReminderManager
    {
        public static Dictionary<ulong, Dictionary<string, List<ReminderNode>>> Reminders;

        public static void LoadReminders() {
            string json = FileManager.ReadFullFile(DataFileNames.UserRemindersFile);
            Reminders = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, List<ReminderNode>>>>(json);
            KLog.Info("Reminders Loaded");
        }

        public static void SaveReminders() {
            BotUtils.WriteToJson(Reminders, DataFileNames.UserRemindersFile);
        }

        public static ReminderNode GetReminder(ReminderPointer rp) {
            return Reminders[rp.User][rp.Date][rp.Index];
        }
    }
}
