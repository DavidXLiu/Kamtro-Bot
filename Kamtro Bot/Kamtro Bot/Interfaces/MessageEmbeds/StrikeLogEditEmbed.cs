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
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class StrikeLogEditEmbed : MessageEmbed
    {
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
        [InputField("User ID (Required)", 3, 1)]
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

            Moderator = ctx.User;

            // Initialize List
            if(Options == null) {
                Options = new List<List<string>>();

                List<string> page = new List<string>();
                page.Add("Add a strike/ban");
                page.Add("Edit a reason");
                page.Add("Delete strike/ban");

                Options.Add(page);

                page = new List<string>();

                page.Add("Add strike/ban to user currently in the log");
                page.Add("Add a new entry to the log");

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

            AddMenuOptions(ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.CHECK, ReactionHandler.DONE, ReactionHandler.DECLINE);
            RegisterMenuFields();
        }

        public async override Task PerformAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.DOWN_STR:
                    await MoveCursorDown();
                    await OpCursorDown();
                    break;

                case ReactionHandler.UP_STR:
                    await MoveCursorUp();
                    await OpCursorUp();
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
                    await Message.DeleteAsync();
                    await Context.Message.Channel.SendMessageAsync(BotUtils.KamtroText("Thank you for editing the log."));
                    break;

                case ReactionHandler.CHECK_STR:
                    HandleConfirm();
                    break;

                case ReactionHandler.DONE_STR:
                    if(!BackMap.ContainsKey(PageNum)) {
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

        public override void PerformMessageAction(SocketUserMessage message) {
            base.PerformMessageAction(message); // first start off with the base func

            // now for special rendering
            if (PageNum == 2 || PageNum == 4 || PageNum == 9) {
                // Autofill.
                List<SocketGuildUser> users = BotUtils.GetUsers(ServerUsername);
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

                    if (PageNum == 2) {
                        ServerUsername = user.Mention;
                    } else if (PageNum == 4) {
                        ServerUsername4 = user.Mention;
                    } else if (PageNum == 9) {
                        ServerUsername9 = user.Mention;
                    }

                    Autofill = user;
                }
            }
        }

        #region Helper Methods
        private void HandleConfirm() {
            LastPage = PageNum;

            switch (PageNum) {
                case 1:
                    switch (OpCursor) {
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
                        UserId = Autofill.Id.ToString();
                        FullUsername = BotUtils.GetFullUsername(Autofill);
                        Strike1Reason = AdminDataManager.GetStrikeReason(Autofill.Id, 1);
                        Strike2Reason = AdminDataManager.GetStrikeReason(Autofill.Id, 2);
                        BanReason = AdminDataManager.GetStrikeReason(Autofill.Id, 3);
                    }

                    PageNum = 3;
                    break;

                case 3:
                    ulong id;
                    if(!string.IsNullOrWhiteSpace(UserId) && ulong.TryParse(UserId, out id)) {
                        bool strike1Added = false;
                        bool strike2Added = false;

                        if(!string.IsNullOrWhiteSpace(Strike1Reason)) {
                            AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason));
                            strike1Added = true;
                        }

                        if(!string.IsNullOrWhiteSpace(Strike2Reason)) {
                            if(!strike1Added && AdminDataManager.GetStrikes(id) == 0) {
                                // Add strike 1 if it wasn't added a few lines ago, and it isn't already there
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason));
                                strike1Added = true;
                            }

                            // Add strike 2
                            AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike2Reason));
                            strike2Added = true;
                        }

                        if(!string.IsNullOrWhiteSpace(BanReason)) {
                            if (!strike1Added && AdminDataManager.GetStrikes(id) == 0) {
                                // Add strike 1 if it wasn't added a few lines ago, and it isn't already there
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike1Reason));
                            }

                            if (!strike2Added && AdminDataManager.GetStrikes(id) == 1) {
                                AdminDataManager.AddStrike(id, new Nodes.StrikeDataNode(Context.User, Strike2Reason));
                            }

                            AdminDataManager.AddBan(id, new Nodes.BanDataNode(Context.User, BanReason));
                        }
                    }
                    break;

                case 4:
                    // autofill
                    if (Autofill != null) {
                        UserId = Autofill.Id.ToString();
                        FullUsername = BotUtils.GetFullUsername(Autofill);
                        Strike1Reason6 = AdminDataManager.GetStrikeReason(Autofill.Id, 1);
                        Strike2Reason7 = AdminDataManager.GetStrikeReason(Autofill.Id, 2);
                        BanReason8 = AdminDataManager.GetStrikeReason(Autofill.Id, 3);
                    } else {
                        return; // you must specify a user
                    }

                    PageNum = 5;
                    break;

                case 5:
                    switch (OpCursor) {
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
                    break;

                case 7:
                    // handle confirm
                    AdminDataManager.SetStrikeReason(Autofill.Id, 2, Strike2Reason7);
                    break;

                case 8:
                    // handle confirm
                    AdminDataManager.SetStrikeReason(Autofill.Id, 3, BanReason8);
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
                    switch (OpCursor) {
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

                    break;

                case 12:
                    // handle confirm
                    break;

                case 13:
                    // handle confirm
                    break;
            }
        }
        
        private async Task OpCursorUp() {
            OpCursor--;
            if (PageNum >= Options.Count() || OpCursor < 0) {
                OpCursor = Options[PageNum-1].Count() - 1;
            }

            await UpdateEmbed();
        }

        private async Task OpCursorDown() {
            OpCursor++;
            if (PageNum >= Options.Count() || Options[PageNum-1].Count() - 1 < OpCursor) {
                OpCursor = 0;
            }

            await UpdateEmbed();
        }
        #endregion
    }
}
