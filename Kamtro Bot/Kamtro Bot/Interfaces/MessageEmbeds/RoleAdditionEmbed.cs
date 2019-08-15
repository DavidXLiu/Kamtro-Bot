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
        private bool Remove;

        public RoleAdditionEmbed(SocketCommandContext ctx, SocketRole role, bool remove = false) {
            SetCtx(ctx);

            Role = role;
            Remove = remove;

            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    if(Remove) {
                        await RemRole();
                    } else {
                        await AddRole();
                    }

                    Program.ReloadConfig();

                    EventQueueManager.RemoveEvent(this);
                    await Message.DeleteAsync();
                    break;

                case ReactionHandler.DECLINE_STR:
                    if (Remove) {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Role deletion has been cancelled."));
                    } else {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Role addition has been cancelled."));
                    }

                    EventQueueManager.RemoveMessageEvent(this);
                    await Context.Channel.DeleteMessageAsync(Message);
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"{(Remove ? "Remove":"Add")} Modifiable Role");
            eb.WithColor(BotUtils.Orange);

            eb.AddField("Role", Role.Mention);

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }

        private async Task AddRole() {
            Program.Settings.ModifiableRoles.Add(Role.Id);
            Program.Settings.RoleDescriptions.Add(Role.Id, new RoleInfoNode(Role.Name, Description, Role.Color.ToString()));
            Program.SaveSettings();

            await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Role {Role.Name} has been added to the list of modifiable roles."));
        }

        private async Task RemRole() {
            Program.Settings.ModifiableRoles.Remove(Role.Id);
            Program.Settings.RoleDescriptions.Remove(Role.Id);
            Program.SaveSettings();

            await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Role {Role.Name} has been removed from the list of modifiable roles."));
        }
    }
}
