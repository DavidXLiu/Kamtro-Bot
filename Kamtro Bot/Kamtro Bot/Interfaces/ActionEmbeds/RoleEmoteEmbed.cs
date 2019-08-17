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

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class RoleEmoteEmbed : ActionEmbed
    {
        private const string EmptyOrInvalidEmote = "[Please react with the emote you want associated with this role.]";

        private SocketRole Role;
        private string Emote;
        private bool Remove;

        public RoleEmoteEmbed(SocketCommandContext ctx, SocketRole role) {
            Role = role;
            Emote = BotUtils.ZeroSpace;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Add Role Option");
            eb.WithColor(Role.Color);

            eb.AddField("Role", Role.Mention);
            eb.AddField("Emote", Emote);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            string opt = option.Emote.ToString();

            switch (opt) {
                case ReactionHandler.CHECK_STR:
                    if(ReactionHandler.RoleMap.ContainsKey(opt)) {
                        Emote = $"The role {ServerData.Server.GetRole(ReactionHandler.RoleMap[opt]).Mention} already has that emote!";
                        await UpdateEmbed();
                        break;
                    }

                    ReactionHandler.RoleMap.Add(Emote, Role.Id);
                    ReactionHandler.SaveRoleMap();

                    EventQueueManager.RemoveEvent(this);
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Emote Association Added."));
                    await Message.DeleteAsync();
                    break;

                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveEvent(this);
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Action Canceled."));
                    await Message.DeleteAsync();
                    break;

                default:
                    Emote = option.Emote.ToString();
                    await UpdateEmbed();
                    break;
            }
        }
    }
}
