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
using Kamtro_Bot.Interfaces.ActionEmbeds;
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

        private string ErrorMessage = BotUtils.ZeroSpace;
        private string SuccessMessage = BotUtils.ZeroSpace;

        private SocketGuildUser User;
        private TimeZoneNode UserTimeZone;

        private List<ReminderPointer> ReminderList;
        private ReminderPointer CurrentReminder = null;
        #endregion

        #region Message Fields
        #region Page 3
        [InputField("Reminder Name", 3, 1)]
        public string Name;

        [InputField("Reminder Description", 3, 2)]
        public string Description;

        [InputField("Date (MM/DD/YYYY)", 3, 3)]
        public string Date;

        [InputField("Time (HH:MM AM/PM)", 3, 4)]
        public string Time;
        #endregion
        [InputField("New Reminder Name", 5, 1)]
        public string EName;

        [InputField("New Reminder Desription", 6, 1)]
        public string EDesc;

        [InputField("New Reminder Date", 7, 1)]
        public string EDate;

        [InputField("New Reminder Time", 7, 2)]
        public string ETime;

        #endregion

        public ReminderEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            
            User = BotUtils.GetGUser(ctx);
            UserTimeZone = UserDataManager.GetUserData(User).TimeZone;

            Timeout = new TimeSpan(0, 30, 0);

            RefreshList();

            AddMenuOptions(ReactionHandler.SELECT, new MenuOptionNode(ASTERISK_NEW, "Add"), ReactionHandler.UP, ReactionHandler.DOWN, new MenuOptionNode(ReactionHandler.DECLINE_STR, "Delete"), new MenuOptionNode(PENCIL, "Edit"), ReactionHandler.BACK);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:
                    CursorPos = 1;

                    switch (PageNum) {
                        case 1:
                            if (ReminderList.Count == 0) return;
                            CurrentReminder = ReminderList[CursorPos - 1];
                            PageNum = 2;  // move to next page
                            await UpdateEmbed();
                            break;

                        case 3:
                            if (Regex.IsMatch(Time, @"^[0-2]{0,1}[0-9]:\d\d( [AP]M){0,1}$") && Regex.IsMatch(Date, @"^\d{1,2}/\d{1,2}/\d{1,4}$")) {
                                string[] t;

                                if (Time.Length <= 5) t = Time.Split(':');  // 24 hour clock, so no AM/PM. [XX:XX]
                                else t = Time.Split(':', ' ');  // 12 hour clock, so AM/PM [XX:XX AM]

                                int hour = int.Parse(t[0]);
                                int min = int.Parse(t[1]);
                                

                                if (Time.Length <= 5) {
                                    // if 24 hour, check for valid hour
                                    if (hour > 23) {
                                        ErrorHappened = true;
                                        ErrorMessage += "Please input a valid number for the hour. If using 24 hour clock the hour must be between 0 and 23 inclusive.\n";
                                        await UpdateEmbed();
                                        break;
                                    }
                                } else {
                                    // 12 hour. Verify this way and fix hour
                                    if(hour > 12 || hour < 1) {
                                        ErrorHappened = true;
                                        ErrorMessage += "Please input a valid number for the hour. If you are using a 12 hour clock make sure that the hour is between 1 and 12 inclusive.\n";
                                        await UpdateEmbed();
                                        break;
                                    }

                                    if(t[2].ToLower() == "pm") {
                                        hour += 12;
                                        if (hour == 24) hour = 0;
                                    }
                                }

                                if (min > 59) {
                                    ErrorHappened = true;
                                    ErrorMessage += "Please input a valid number for the minute. It must be a number from 00-59 inclusive.\n";
                                    await UpdateEmbed();
                                    break;
                                }

                                string[] d = Date.Split('/');

                                int month = int.Parse(d[0]);
                                int day = int.Parse(d[1]);
                                int year = int.Parse(d[2]);

                                DateTime dtime = new DateTime(year, month, day, hour, min, 0);

                                dtime = dtime.AddHours(-UserTimeZone.Hour).AddMinutes(-UserTimeZone.Minute);

                                if(dtime < DateTime.UtcNow) {
                                    ErrorHappened = true;
                                    ErrorMessage += "The reminder must be set for a date/time in the future!\n";
                                    await UpdateEmbed();
                                    break;
                                }

                                // All good, so add it.
                                ReminderManager.AddReminder(User.Id, Name, dtime, Description);
                                RefreshList();

                                ModifySuccess = true;
                                SuccessMessage += "The reminder has been added!\n";
                                PageNum = 1;
                            } else {
                                ErrorHappened = true;
                                ErrorMessage += "Your date or time are in an invalid format. Reminders can only be set for dates/times in the future and cannot be set past year 9999.\n";
                            }

                            await UpdateEmbed();
                            break;

                        case 4:
                            switch(CursorPos) {
                                case 1:
                                    PageNum = 5;
                                    break;

                                case 2:
                                    PageNum = 6;
                                    break;

                                case 3:
                                    PageNum = 7;
                                    break;

                                default:
                                    CursorPos = 1;
                                    break;
                            }

                            await UpdateEmbed();
                            break;

                        case 5:
                            // Edit Name
                            if (string.IsNullOrWhiteSpace(EName)) EName = BotUtils.ZeroSpace;
                            ReminderManager.EditReminder(CurrentReminder, EName);

                            ModifySuccess = true;
                            SuccessMessage += "The reminder has been updated!\n";

                            PageNum = 4;
                            await UpdateEmbed();
                            break;

                        case 6:
                            // Edit Desc
                            if (string.IsNullOrWhiteSpace(EDesc)) EDesc = BotUtils.ZeroSpace;
                            ReminderManager.EditReminder(CurrentReminder, newDesc: EDesc);

                            ModifySuccess = true;
                            SuccessMessage += "The reminder has been updated!\n";

                            PageNum = 4;
                            await UpdateEmbed();
                            break;

                        case 7:
                            // Edit Time
                            if (Regex.IsMatch(ETime, @"^[0-2]{0,1}[0-9]:\d\d( [AP]M){0,1}$") && Regex.IsMatch(EDate, @"^\d{1,2}/\d{1,2}/\d{1,4}$")) {
                                string[] t;

                                if (ETime.Length <= 5) t = ETime.Split(':');  // 24 hour clock, so no AM/PM. [XX:XX]
                                else t = ETime.Split(':', ' ');  // 12 hour clock, so AM/PM [XX:XX AM]

                                int hour = int.Parse(t[0]);
                                int min = int.Parse(t[1]);


                                if (Time.Length <= 5) {
                                    // if 24 hour, check for valid hour
                                    if (hour > 23) {
                                        ErrorHappened = true;
                                        ErrorMessage += "Please input a valid number for the hour. If using 24 hour clock the hour must be between 0 and 23 inclusive.\n";
                                        await UpdateEmbed();
                                        break;
                                    }
                                } else {
                                    // 12 hour. Verify this way and fix hour
                                    if (hour > 12 || hour < 1) {
                                        ErrorHappened = true;
                                        ErrorMessage += "Please input a valid number for the hour. If you are using a 12 hour clock make sure that the hour is between 1 and 12 inclusive.\n";
                                        await UpdateEmbed();
                                        break;
                                    }

                                    if (t[2].ToLower() == "pm") {
                                        hour += 12;
                                        if (hour == 24) hour = 0;
                                    }
                                }

                                if (min > 59) {
                                    ErrorHappened = true;
                                    ErrorMessage += "Please input a valid number for the minute. It must be a number from 00-59 inclusive.\n";
                                    await UpdateEmbed();
                                    break;
                                }

                                string[] d = EDate.Split('/');

                                int month = int.Parse(d[0]);
                                int day = int.Parse(d[1]);
                                int year = int.Parse(d[2]);

                                DateTime dtime = new DateTime(year, month, day, hour, min, 0);

                                dtime.AddHours(-UserTimeZone.Hour).AddMinutes(-UserTimeZone.Minute);

                                if (dtime < DateTime.UtcNow) {
                                    ErrorHappened = true;
                                    ErrorMessage += "The reminder must be set for a date/time in the future!\n";
                                    await UpdateEmbed();
                                    break;
                                }

                                // All good, so add it.
                                ReminderManager.EditReminder(CurrentReminder, newDate: dtime.ToString());
                                RefreshList();

                                ModifySuccess = true;
                                SuccessMessage += "The reminder has been updated!\n";
                            } else {
                                ErrorHappened = true;
                                ErrorMessage += "Your date or time are in an invalid format. Reminders can only be set for dates/times in the future and cannot be set past year 9999.\n";
                            }

                            PageNum = 4;
                            await UpdateEmbed();
                            break;
                    }
                    break;

                case ASTERISK_NEW:
                    CursorPos = 1;

                    if (PageNum >= 4 && PageNum <= 7) break;  // if on edit embed pages, don't do anything.
                    PageNum = 3;
                    await UpdateEmbed();
                    break;

                case ReactionHandler.DECLINE_STR:
                    if (ReminderList.Count == 0) return;
                    
                    if(PageNum == 1) CurrentReminder = ReminderList[CursorPos - 1];

                    ConfirmEmbed ce = new ConfirmEmbed(Context, $"Are you sure you want to delete the reminder {ReminderManager.GetReminder(CurrentReminder).Name}?", DeleteRemidnerConfirm);

                    switch (PageNum) {
                        case 1:
                        case 2:
                            await ce.Display();
                            break;
                    }
                    break;

                case ReactionHandler.BACK_STR:
                    switch(PageNum) {
                        case 1:
                            return;

                        case 2:
                        case 3:
                        case 4:
                            PageNum = 1;
                            break;

                        case 5:
                        case 6:
                        case 7:
                            PageNum = 4;
                            break;
                    }

                    CursorPos = 1;

                    await UpdateEmbed();
                    break;

                case PENCIL:
                    if(PageNum == 1) {
                        CurrentReminder = ReminderList[CursorPos - 1];
                        PageNum = 4;
                        await UpdateEmbed();
                    } else if(PageNum == 2) {
                        PageNum = 4;
                        await UpdateEmbed();
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

                        dt = d.ToString("dd/MM/yyyy") + " at " + d.ToString("hh:mm tt");
                    }

                    eb.AddField("Name", name);
                    eb.AddField("Time", dt);
                    eb.AddField("Description", desc);
                    break;

                case 3:
                    eb.WithTitle("Add Reminder");
                    eb.WithDescription("When adding a reminder, make sure that you add a space in between the time, and the AM/PM if you are using a 12 hour clock. AM/PM are not nessecary in a 24 hour clock.\nDates MUST be in MM/DD/YYYY format. Months are in number form, January is 1 (or 01, single digit numbers can have a single zero in front if you want), December is 12.");
                    break;

                case 4:
                    eb.WithTitle("Edit Remidner");

                    string options = $"{(CursorPos == 1 ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}Edit Name\n";
                    options += $"{(CursorPos == 2 ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}Edit Description\n";
                    options += $"{(CursorPos == 3 ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}Edit Date/Time";

                    eb.AddField("Options", options);
                    break;

                case 5:
                case 6:
                case 7:
                    eb.WithTitle("Edit Remidner");
                    break;
            }

            AddEmbedFields(eb);
            AddMenu(eb);

            if(ModifySuccess) {
                eb.AddField("Success", SuccessMessage);

                ModifySuccess = false;
                SuccessMessage = BotUtils.ZeroSpace;
            }

            if(ErrorHappened) {
                eb.AddField("ERROR", ErrorMessage);

                ErrorHappened = false;
                ErrorMessage = BotUtils.ZeroSpace;
            }

            return eb.Build();
        }

        private string MakeBold(string s, bool bold) {
            if (bold) return $"**{s}**";
            else return s;
        }

        private string GetDateString(ReminderPointer rp) {
            DateTime dt = DateTime.Parse(rp.Date);

            return dt.AddHours(UserTimeZone.Hour).AddMinutes(UserTimeZone.Minute).ToString("MM/dd/yyyy h:mm tt");
        }

        protected override async Task MoveCursorDown(int num = 1) {
            switch(PageNum) {
                case 1:
                    if (ReminderList.Count == 0) return;
                    CursorPos++;
                    if (CursorPos > ReminderList.Count) CursorPos = 1;
                    await UpdateEmbed();
                    break;

                case 2:
                    break;

                case 4:
                    CursorPos++;
                    if (CursorPos > 3) CursorPos = 1;
                    await UpdateEmbed();
                    break;

                default:
                    await base.MoveCursorDown(num);
                    break;

            }
        }

        protected override async Task MoveCursorUp(int num = 1) {
            switch(PageNum) {
                case 1:
                    if (ReminderList.Count == 0) return;
                    CursorPos--;
                    if (CursorPos < 1) CursorPos = ReminderList.Count;
                    await UpdateEmbed();
                    break;

                case 2:
                    break;

                case 4:
                    CursorPos--;
                    if (CursorPos < 1) CursorPos = 3;
                    await UpdateEmbed();
                    break;

                default:
                    await base.MoveCursorUp(num);
                    break;

            }
        }

        public async Task DeleteRemidnerConfirm(bool b) {
            if(b) {
                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Reminder successfully deleted."));
                ReminderManager.DeleteReminder(CurrentReminder);
                RefreshList();
                CursorPos = 1;
                await UpdateEmbed();
            }
        }

        private void RefreshList() {
            ReminderList = ReminderManager.GetAllRemindersForUser(User);
        }
    }
}
