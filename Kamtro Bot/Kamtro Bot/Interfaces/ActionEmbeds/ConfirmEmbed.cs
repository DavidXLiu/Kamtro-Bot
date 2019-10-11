using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class ConfirmEmbed : ActionEmbed
    {
        private Func<bool, Task> Action;
        private string BoxText;

        public ConfirmEmbed(string txt = "Are you sure?", Func<bool, Task> func = null) {
            Action = func;
            BoxText = txt;

            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Confirm Action");
            eb.WithColor(BotUtils.Green);

            if (Action == null) {
                eb.WithDescription("Something went wrong! Please ping arcy and carbon.");
            } else {
                eb.WithDescription(BoxText);
            }

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    await Action(true);
                    EventQueueManager.RemoveEvent(this);
                    await Message.DeleteAsync();
                    break;

                case ReactionHandler.DECLINE_STR:
                    await Action(false);
                    EventQueueManager.RemoveEvent(this);
                    await Message.DeleteAsync();
                    break;
            }
        }
    }
}
