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
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class RoleAdditionEmbed : MessageEmbed
    {
        [InputField("Role Description", 1, 1)]
        public string Description;

        private SocketRole Role;

        public RoleAdditionEmbed(SocketCommandContext ctx, SocketRole role) {
            SetCtx(ctx);

            Role = role;

            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    Program.Settings.ModifiableRoles.Add(Role.Id);
                    Program.Settings.RoleDescriptions.Add(Role.Id, new RoleInfoNode(Role.Name, Description, Role.Color.ToString()));
                    Program.SaveSettings();

                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Role {Role.Name} has been added to the list of modifyable roles."));
                    break;

                case ReactionHandler.DECLINE_STR:
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Role addition has been cancelled."));

                    EventQueueManager.RemoveMessageEvent(this);
                    await Context.Channel.DeleteMessageAsync(Message);
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Add Modifiable Role");
            eb.WithColor(BotUtils.Orange);

            eb.AddField("Role", Role.Mention);

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
