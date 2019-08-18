using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
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

        public RoleEmoteEmbed(SocketCommandContext ctx, SocketRole role) {
            SetCtx(ctx);

            Role = role;
            Emote = BotUtils.ZeroSpace;

            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Add Role Option");
            eb.WithColor(Role.Color);

            eb.AddField("Role", Role.Mention);
            //eb.AddField(BotUtils.ZeroSpace, "React with an emote to assign that emote to this role.");
            eb.AddField("Emote", Emote);

            eb.WithDescription("React with an emote to assign that emote to this role.");

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            string opt = option.Emote.ToString();

            switch (opt) {
                case ReactionHandler.CHECK_STR:
                    if(Emote == BotUtils.ZeroSpace || Emote == EmptyOrInvalidEmote) {
                        Emote = EmptyOrInvalidEmote;
                        await UpdateEmbed();
                        return;
                    }

                    if(ReactionHandler.RoleMap.ContainsKey(Emote)) {
                        Emote = $"The role {ServerData.Server.GetRole(ReactionHandler.RoleMap[Emote]).Mention} already has that emote!";
                        await UpdateEmbed();
                        break;
                    }

                    ReactionHandler.RoleMap.Add(Emote, Role.Id);
                    ReactionHandler.SaveRoleMap();

                    await UpdateRoleMessage();

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

        /// <summary>
        /// Method used to update the bot message that contains the list of modifiable roles.
        /// Users can react to this message to get or remove a role with its corresponding reaction.
        /// Call this method whenever the list of modifiable roles changes.
        /// Author: Arcy
        /// </summary>
        /// <returns></returns>
        private async Task UpdateRoleMessage()
        {
            IMessage roleMessage = await ServerData.Server.GetTextChannel(Program.Settings.RoleSelectChannelID).GetMessageAsync(Program.Settings.RoleSelectMessageID);

            // Form message with each pair in the role map
            string message = "";
            foreach (KeyValuePair<string, ulong> pair in ReactionHandler.RoleMap)
            {
                message += ServerData.Server.GetRole(pair.Value).Mention + " - " + pair.Key + "\n";
            }
            await (roleMessage as RestUserMessage).ModifyAsync(x => x.Content = message);
        }
    }
}
