using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class StrikeLogEditEmbed : MessageEmbed
    {
        private const string DefaultText = "[Enter Text]";
        private const string BACK = "\U0001F519";

        private static List<List<string>> Options = null;
        private static Dictionary<int, int> BackMap = null;

        private int OpCursor = 0;
        private int LastPage = 1;

        private SocketUser Moderator;
        private SocketUser Autofill = null;

        private string Strike1Reason11;
        private string Strike2Reason12;
        private string BanReason13;
        


        #region Input Fields
        [InputField("Enter Server User", 2, 1)]
        public string ServerUsername;

        #region Page 3
        [InputField("User ID (Required)", 3, 1, Type = FieldDataType.ULONG)]
        public string UserId;

        [InputField("Full Username + Descriminator", 3, 2)]
        public string FullUsername;

        [InputField("Strike 1 Reason", 3, 3)]
        public string Strike1Reason;

        [InputField("Strike 2 Reason", 3, 4)]
        public string Strike2Reason;

        [InputField("Ban Reason", 3, 5)]
        public string BanReason;
        #endregion


        [InputField("Enter Server User", 4, 1)]
        public string ServerUsername4;

        #region Page 6-8
        [InputField("Strike 1 Reason", 6, 1)]
        public string Strike1Reason6;
        [InputField("Strike 2 Reason", 7, 1)]
        public string Strike2Reason7;
        [InputField("Ban Reason", 8, 1)]
        public string BanReason8;
        #endregion

        [InputField("Enter Server User", 9, 1)]
        public string ServerUsername9;
        #endregion

        public StrikeLogEditEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            Timeout = new TimeSpan(0, 20, 0);
            Moderator = ctx.User;

            // Initialize List
            if(Options == null) {
                Options = new List<List<string>>();

                // Page 1
                List<string> page = new List<string>();
                page.Add("Add a strike/ban");
                page.Add("Edit a reason");
                page.Add("Delete strike/ban");

                Options.Add(page);

                // Page 2-4
                Options.Add(new List<string>());
                Options.Add(new List<string>());
                Options.Add(new List<string>());

                // Page 5
                page = new List<string>();

                page.Add("Strike 1 Reason");
                page.Add("Strike 2 Reason");
                page.Add("Ban Reason");

                Options.Add(page);

                // Pages 6-9
                Options.Add(new List<string>());
                Options.Add(new List<string>());
                Options.Add(new List<string>());
                Options.Add(new List<string>());

                // Page 10
                page = new List<string>();

                page.Add("Strike 1 Reason");
                page.Add("Strike 2 Reason");
                page.Add("Ban Reason");

                Options.Add(page);
            }

            if(BackMap == null) {
                BackMap = new Dictionary<int, int>();

                BackMap.Add(3, 2);
                BackMap.Add(2, 1);
                BackMap.Add(4, 1);
                BackMap.Add(5, 4);
                BackMap.Add(6, 5);
                BackMap.Add(7, 5);
                BackMap.Add(8, 5);
                BackMap.Add(9, 1);
                BackMap.Add(10, 9);
                BackMap.Add(11, 10);
                BackMap.Add(12, 10);
                BackMap.Add(13, 10);
                BackMap.Add(14, 1);
            }

            RegisterMenuFields();
            AddMenuOptions(new MenuOptionNode(BACK, "Go Back"), ReactionHandler.CHECK, ReactionHandler.DECLINE);
        }

        public async override Task PerformAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.DOWN_STR:
                    await MoveCursorUp();
                    await OpCursorUp();
                    break;

                case ReactionHandler.UP_STR:
                    await MoveCursorDown();
                    await OpCursorDown();
                    break;

                default:
                    await ButtonAction(action);
                    break;
            }
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch (action.Emote.ToString()) {
                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveMessageEvent(Context.User.Id);
                    await Context.Message.Channel.SendMessageAsync(BotUtils.KamtroText("Thank you for editing the log."));
                    await Message.DeleteAsync();
                    return;

                case ReactionHandler.CHECK_STR:
                    HandleConfirm();
                    OpCursor = 0; // reset the cursor
                    break;

                case BACK:
                    if(!BackMap.ContainsKey(PageNum) || PageNum == 14) {
                        PageNum = 1;
                        break;
                    }

                    PageNum = BackMap[PageNum];
                    break;
            }

            await UpdateEmbed();
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Editing Strike Log");
            eb.WithColor(BotUtils.Kamtro);

            if(PageNum == 14) {
                eb.AddField("Thank you for editing the stirke log", "Press the back button to return to the start, or press cancel if you have no more edits to make.");
                AddMenu(eb);
                return eb.Build();
            }

            // now for the reason fields
            if (PageNum == 11) {
                eb.AddField("Strike 1 reason", Strike1Reason11);
            } else if(PageNum == 12) {
                eb.AddField("Strike 2 reason", Strike2Reason12);
            } else if(PageNum == 13) {
                eb.AddField("Ban Reason", BanReason13);
            }

            if(PageNum-1 < Options.Count() && Options[PageNum-1].Count() > 0) {
                string text = "";

                if (OpCursor >= Options[PageNum - 1].Count()) {
                    OpCursor = 0;
                }

                for (int i = 0; i < Options[PageNum-1].Count(); i++) {
                    if(i == OpCursor) {
                        text += $"{CustomEmotes.CursorAnimated} {i+1}. {Options[PageNum - 1][i]}\n";
                    } else {
                        text += $"{i + 1}. {Options[PageNum - 1][i]}\n";
                    }
                }

                text.TrimEnd('\n', '\r');

                eb.AddField("Please select an option", text);
            }

            AddMenu(eb);
            AddEmbedFields(eb);

            return eb.Build();
        }

        public override async Task PerformMessageAction(SocketUserMessage message) {
            await base.PerformMessageAction(message); // first start off with the base func

            // now for special rendering
            if (PageNum == 2 || PageNum == 4 || PageNum == 9) {
                // Autofill.
                List<SocketGuildUser> users;

                switch(PageNum) {
                    case 2:
                        users = BotUtils.GetUsers(ServerUsername);
                        break;

                    case 4:
                        users = BotUtils.GetUsers(ServerUsername4);
                        break;

                    case 9:
                        users = BotUtils.GetUsers(ServerUsername9);
                        break;

                    default:
                        users = new List<SocketGuildUser>();
                        break;
                }
                
                SocketGuildUser user;

                if (users.Count < 1) {
                    // fail check
                    if (PageNum == 2) {
                        ServerUsername = $"Unable to find user: {ServerUsername}";
                    } else if (PageNum == 4) {
                        ServerUsername4 = $"Unable to find user: {ServerUsername4}";
                    } else if (PageNum == 9) {
                        ServerUsername9 = $"Unable to find user: {ServerUsername9}";
                    }

                    Autofill = null;
                } else {
                    user = users[0];

                    if (PageNum == 2 || PageNum == 4 || PageNum == 9) {
                        InputFields[PageNum][CursorPos].SetValue(user.Mention);
                    }

                    Autofill = user;
                }

                await UpdateEmbed();
            }
        }

        #region Helper Methods
        private void HandleConfirm() {
            LastPage = PageNum;

            switch (PageNum) {
                case 1:
                    switch (OpCursor+1) {
                        case 1:
                            PageNum = 2;
                            break;

                        case 2:
                            PageNum = 4;
                            break;

                        case 3:
                            PageNum = 9;
                            break;
                    }
                    break;

                case 2:
                    // autofill
                    if(Autofill != null) {
                        InputFields[3][1].SetValue(Parse(Autofill.Id.ToString()));
                        InputFields[3][2].SetValue(Parse(BotUtils.GetFullUsername(Autofill)));
                        InputFields[3][3].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 1)));
                        InputFields[3][4].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 2)));
                        InputFields[3][5].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 3)));
                    }

                    PageNum = 3;
                    break;

                case 3:
                    ulong id;
                    if(!string.IsNullOrWhiteSpace(UserId) && ulong.TryParse(UserId, out id)) {
                        bool strike1Added = false;
                        bool strike2Added = false;

                        if(!string.IsNullOrWhiteSpace(Strike1Reason) && Strike1Reason != DefaultText) {
                            AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason), BotUtils.GetFullUsername(Context.User));
                            strike1Added = true;
                        }

                        if(!string.IsNullOrWhiteSpace(Strike2Reason) && Strike2Reason != DefaultText) {
                            if(!strike1Added && AdminDataManager.GetStrikes(id) == 0) {
                                // Add strike 1 if it wasn't added a few lines ago, and it isn't already there
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason), BotUtils.GetFullUsername(Context.User));
                                strike1Added = true;
                            }

                            // Add strike 2
                            AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike2Reason), BotUtils.GetFullUsername(Context.User));
                            strike2Added = true;
                        }

                        if(!string.IsNullOrWhiteSpace(BanReason) && BanReason != DefaultText) {
                            if (!strike1Added && AdminDataManager.GetStrikes(id) == 0) {
                                // Add strike 1 if it wasn't added a few lines ago, and it isn't already there
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason), BotUtils.GetFullUsername(Context.User));
                            }

                            if (!strike2Added && AdminDataManager.GetStrikes(id) == 1) {
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike2Reason), BotUtils.GetFullUsername(Context.User));
                            }

                            AdminDataManager.AddBan(id, new Nodes.BanDataNode(Context.User, BanReason), BotUtils.GetFullUsername(Context.User));
                        }

                        PageNum = 14;
                    }
                    break;

                case 4:
                    // autofill
                    if (Autofill != null) {
                        UserId = Autofill.Id.ToString();
                        FullUsername = BotUtils.GetFullUsername(Autofill);
                        InputFields[6][1].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 1)));
                        InputFields[7][1].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 2)));
                        InputFields[8][1].SetValue(Parse(AdminDataManager.GetStrikeReason(Autofill.Id, 3)));
                    } else {
                        return; // you must specify a user
                    }

                    PageNum = 5;
                    break;

                case 5:
                    switch (OpCursor+1) {
                        case 1:
                            PageNum = 6;
                            break;

                        case 2:
                            PageNum = 7;
                            break;

                        case 3:
                            PageNum = 8;
                            break;
                    }
                    break;

                case 6:
                    // handle confirm
                    AdminDataManager.SetStrikeReason(Autofill.Id, 1, Strike1Reason6);
                    PageNum = 14;
                    break;

                case 7:
                    // handle confirm
                    AdminDataManager.SetStrikeReason(Autofill.Id, 2, Strike2Reason7);
                    PageNum = 14;
                    break;

                case 8:
                    // handle confirm
                    AdminDataManager.SetStrikeReason(Autofill.Id, 3, BanReason8);
                    PageNum = 14;
                    break;

                case 9:
                    // HC
                    if (Autofill != null) {
                        UserId = Autofill.Id.ToString();
                        FullUsername = BotUtils.GetFullUsername(Autofill);
                        Strike1Reason11 = AdminDataManager.GetStrikeReason(Autofill.Id, 1);
                        Strike2Reason12 = AdminDataManager.GetStrikeReason(Autofill.Id, 2);
                        BanReason13  = AdminDataManager.GetStrikeReason(Autofill.Id, 3);
                    } else {
                        return; // you must specify a user
                    }

                    PageNum = 10;
                    break;

                case 10:
                    switch (OpCursor+1) {
                        case 1:
                            PageNum = 11;
                            break;

                        case 2:
                            PageNum = 12;
                            break;

                        case 3:
                            PageNum = 13;
                            break;
                    }
                    break;

                case 11:
                    // handle confirm
                    AdminDataManager.DeleteStrike(Autofill.Id, 1);
                    PageNum = 14;
                    break;

                case 12:
                    // handle confirm
                    AdminDataManager.DeleteStrike(Autofill.Id, 2);
                    PageNum = 14;
                    break;

                case 13:
                    // handle confirm
                    AdminDataManager.DeleteStrike(Autofill.Id, 3);
                    PageNum = 14;
                    break;
            }
        }
        
        private async Task OpCursorUp() {
            OpCursor++;
            if (PageNum-1 >= Options.Count() || Options[PageNum - 1].Count() - 1 < OpCursor) {
                OpCursor = 0;
            }

            await UpdateEmbed();
        }

        private async Task OpCursorDown() {
            OpCursor--;
            if (PageNum-1 >= Options.Count() || OpCursor < 0) {
                OpCursor = Options[PageNum - 1].Count() - 1;
            }

            await UpdateEmbed();
        }

        private string Parse(string inp) {
            if(string.IsNullOrWhiteSpace(inp)) {
                return "[Enter Text]";
            }

            return inp;
        }
        #endregion
    }
}
