using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    /// <summary>
    /// Reminder Embed Class
    /// </summary>
    /// <remarks>
    /// TODO: 
    /// Implement pages according to flow chart
    /// </remarks>
    public class ReminderEmbed : MessageEmbed {
        #region Variables
        public const string PENCIL = "✏️";
        public const string ASTERISK_NEW = "\u2733\uFE0F";

        private bool ModifySuccess = false;
        private bool ErrorHappened = false;

        private string ErrorMessage = "";
        private int PageStore = 1;
        private SocketGuildUser User;

        private List<ReminderPointer> ReminderList;
        private ReminderPointer CurrentReminder = null;
        #endregion

        #region Message Fields
        #region Page 3
        [InputField("Reminder Name", 3, 1)]
        public string Name;

        [InputField("Reminder Description", 3, 2)]
        public string Description;

        [InputField("Date (DD/MM/YYYY)", 3, 3)]
        public string Date;

        [InputField("Time (HH:MM AM/PM)", 3, 4)]
        public string Time;
        #endregion
        #endregion

        public ReminderEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            User = BotUtils.GetGUser(ctx);

            ReminderList = ReminderManager.GetAllRemindersForUser(User);

            AddMenuOptions(ReactionHandler.SELECT, new MenuOptionNode(ASTERISK_NEW, "Add"), ReactionHandler.UP, ReactionHandler.DOWN, new MenuOptionNode(ReactionHandler.DECLINE_STR, "Delete"), new MenuOptionNode(PENCIL, "Edit"), ReactionHandler.BACK);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:
                    switch(PageNum) {
                        case 1:
                            CurrentReminder = ReminderList[CursorPos - 1];
                            PageNum = 2;  // move to next page
                            await UpdateEmbed();
                            break;

                        case 3:
                            // TODO: HANDLE TIME ZONES HERE
                            await UpdateEmbed();
                            break;
                    }
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("Reminders");
            eb.WithColor(BotUtils.Kamtro);

            switch(PageNum) {
                case 1:
                    string names = "";
                    string descs = "";
                    string dates = "";

                    for(int i = 0; i < ReminderList.Count; i++) {
                        ReminderPointer rp = ReminderList[i];
                        ReminderNode node = ReminderManager.GetReminder(rp);

                        names += $"{(CursorPos == i + 1 ? CustomEmotes.CursorAnimated:"")}{MakeBold(node.Name, CursorPos == i + 1)}\n";
                        descs += MakeBold(BotUtils.ShortenLongString(node.Description, 20), CursorPos == i + 1) + "\n";
                        dates += MakeBold(GetDateString(rp), CursorPos == i + 1) + "\n";
                    }

                    if(ReminderList.Count == 0) {
                        eb.AddField("Reminders", "You don't have any reminders set! Click the add reminder button to add one!");
                    } else {
                        eb.AddField("Name", names, true);
                        eb.AddField("Description", descs, true);
                        eb.AddField("Date M/D/Y", dates, true);
                    }

                    break;

                case 2:
                    string name, desc, dt;

                    if(CurrentReminder == null) {
                        name = "ERROR";
                        desc = "Something went wrong! Please contact arcy or carbon!\nTry clicking the back button!";
                        dt = "NONE";
                    } else {
                        name = ReminderManager.GetReminder(CurrentReminder).Name;
                        desc = ReminderManager.GetReminder(CurrentReminder).Description;

                        DateTime d = DateTime.Parse(CurrentReminder.Date);
                        TimeZoneNode node = UserDataManager.GetUserData(BotUtils.GetGUser(CurrentReminder.User)).TimeZone;

                        d.AddHours(node.Hour);
                        d.AddMinutes(node.Minute);

                        dt = d.ToString("dd/MM/yyyy hh:mm tt");
                    }

                    eb.AddField("Name", name);
                    eb.AddField("Time", dt);
                    eb.AddField("Description", desc);
                    break;

                case 3:
                    eb.WithDescription("When adding a reminder, make sure that you add a space in between the time, and the AM/PM if you are using a 12 hour clock. AM/PM are not nessecary in a 24 hour clock");

                    if(Regex.IsMatch(Time, @"[0-2]{0,1}[0-9]:\d\d ([AP]M){0,1}")) {
                        if (Time.Length <= 5) {
                            // 24 hour clock
                            string[] t = Time.Split(':');
                            int hour = int.Parse(t[0]);
                            int min = int.Parse(t[1]);

                            if (hour > 23) {
                                ErrorHappened = true;
                                ErrorMessage += "Please input a valid number for the hour. If using 24 hour clock the hour must be between 0 and 23 inclusive.\n";
                            }
                        }
                    }
                    
                    break;
            }

            AddEmbedFields(eb);
            AddMenu(eb);

            if(ModifySuccess) {
                eb.AddField("Success", "The action has been performed successfully");
            }

            return eb.Build();
        }

        private string MakeBold(string s, bool bold) {
            if (bold) return $"**{s}**";
            else return s;
        }

        private string GetDateString(ReminderPointer rp) {
            DateTime dt = DateTime.Parse(rp.Date);

            return dt.ToString("MM/dd/yyyy h:mm tt");
        }
    }
}
