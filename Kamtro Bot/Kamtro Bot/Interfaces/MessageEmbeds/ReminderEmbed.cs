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

        private bool ModifySuccess = false;
        private SocketGuildUser User;

        private List<ReminderPointer> ReminderList;

        #endregion

        #region Message Fields
        #region Page 2
        #endregion
        #endregion

        public ReminderEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            User = BotUtils.GetGUser(ctx);



            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, new MenuOptionNode(ReactionHandler.DECLINE_STR, "Delete"), new MenuOptionNode(PENCIL, "Edit"), ReactionHandler.BACK);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.SELECT_STR:

                    await UpdateEmbed();
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
