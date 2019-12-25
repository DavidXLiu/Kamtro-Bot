using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
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


            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.BACK);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                
            }
        }

        public override Embed GetEmbed() {
            throw new NotImplementedException();
        }

        private ReminderPointer GetRP() {
            ulong id = User.Id;


            return null;
        }
    }
}
