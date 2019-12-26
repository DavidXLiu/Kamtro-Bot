using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public static class ReminderManager {
        public static Dictionary<ulong, Dictionary<string, List<ReminderNode>>> Reminders;

        public static void LoadReminders() {
            string json = FileManager.ReadFullFile(DataFileNames.UserRemindersFile);
            Reminders = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, List<ReminderNode>>>>(json) ?? new Dictionary<ulong, Dictionary<string, List<ReminderNode>>>();
            KLog.Info("Reminders Loaded");
        }

        public static void SaveReminders() {
            BotUtils.WriteToJson(Reminders, DataFileNames.UserRemindersFile);
        }

        public static ReminderNode GetReminder(ReminderPointer rp) {
            return Reminders[rp.User][rp.Date][rp.Index];
        }

        public static List<ReminderPointer> GetAllRemindersForUser(ulong user) {
            // return a list of pointers to all reminders
            List<ReminderPointer> rps = new List<ReminderPointer>();

            foreach(string date in Reminders[user].Keys) {
                for(int i = 0; i < Reminders[user][date].Count; i++) {
                    rps.Add(new ReminderPointer(user, date, i));
                }
            }
            
            return rps;
        }

        public static List<ReminderPointer> GetAllRemindersForUser(SocketGuildUser user) {
            return GetAllRemindersForUser(user.Id);
        }

        public static void AddReminder(ulong user, string name, DateTime time, string desc) {
            if (string.IsNullOrWhiteSpace(name)) name = BotUtils.ZeroSpace;
            if (string.IsNullOrWhiteSpace(desc)) desc = BotUtils.ZeroSpace;

            ReminderNode node = new ReminderNode(name, desc);

            string dtime = time.ToString();

            if (!Reminders.ContainsKey(user)) Reminders.Add(user, new Dictionary<string, List<ReminderNode>>());
            if (!Reminders[user].ContainsKey(dtime)) Reminders[user].Add(dtime, new List<ReminderNode>());
            Reminders[user][dtime].Add(node);

            SaveReminders();
        }

        public static void EditReminder(ReminderPointer rp, string newName = null, string newDesc = null, string newDate = null) {
            ReminderNode node = GetReminder(rp);

            node.Name = newName ?? node.Name;
            node.Description = newDesc ?? node.Description;

            if (newDate != null) {
                Reminders[rp.User][rp.Date].RemoveAt(rp.Index);

                if (Reminders[rp.User][rp.Date].Count == 0) Reminders[rp.User].Remove(rp.Date);

                Reminders[rp.User].Add(newDate, new List<ReminderNode>());
                Reminders[rp.User][newDate].Add(node);
            }

            SaveReminders();
        }

        public static void DeleteReminder(ReminderPointer rp) {
            Reminders[rp.User][rp.Date].RemoveAt(rp.Index);

            if (Reminders[rp.User][rp.Date].Count == 0) {
                Reminders[rp.User].Remove(rp.Date);
            }

            SaveReminders();
        }

        public static async Task RemindUser(ReminderPointer rp) {
            ReminderNode node = GetReminder(rp);

            SocketGuildUser user = BotUtils.GetGUser(rp.User);

            IDMChannel channel = await user.GetOrCreateDMChannelAsync();

            await channel.SendMessageAsync(embed: new BasicEmbed("Reminder!", node.Description, node.Name, BotUtils.Kamtro).GetEmbed());
        }
    }
}
