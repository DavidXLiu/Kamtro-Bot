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
    public class AddAdminEmbed : ActionEmbed
    {
        private SocketUser User;

        public AddAdminEmbed(SocketUser user) {
            User = user;
            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Admin Addition Confirmation");
            eb.WithColor(BotUtils.Red);

            eb.AddField(BotUtils.ZeroSpace, "Are you sure?");

            eb.AddField("Member to Add", BotUtils.GetFullUsername(User));

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    Program.Settings.AdminUsers.Add(User.Id);
                    Program.Settings.SaveJson();

                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Added user {BotUtils.GetFullUsername(User)} as an admin."));
                    KLog.Important($"User {BotUtils.GetFullUsername(User)} added as an admin");

                    EventQueueManager.RemoveEvent(this);
                    await Message.DeleteAsync();
                    break;

                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveEvent(this);
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Action Canceled."));
                    await Message.DeleteAsync();
                    break;
            }
        }
    }
}
