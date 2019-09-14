using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Util;
using Kamtro_Bot.Nodes;
using Discord.Commands;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Interfaces.BasicEmbeds;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class StrikeEmbed : MessageEmbed
    {
        [InputField("Reason", 1, 1)]
        public string Reason;

        private SocketUser Target;
        private bool notifyUser = true;

        private const string diamond = "\U0001F4A0";

        public StrikeEmbed(SocketCommandContext ctx, SocketUser target) {
            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE, new MenuOptionNode(diamond, "Toggle Notify User"));
            SetCtx(ctx);
            Target = target;
            RegisterMenuFields();

            // Set the channel to take input from only that channel - Arcy
            CommandChannel = ctx.Channel as SocketChannel;
        }

        public override async Task PerformAction(SocketReaction action)
        {
            switch (action.Emote.ToString())
            {
                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveMessageEvent(this);
                    await Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"The strike on {BotUtils.GetFullUsername(Target)} has been cancelled."));
                    break;

                default:
                    await ButtonAction(action);  // if none of the predefined actions were used, it must be a custom action.
                    break;
            }
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    // On confirm
                    if (!AllFieldsFilled()) return;  // Do nothing if the fields are not filled.

                    // otherwise, add the strike.
                    StrikeDataNode str = new StrikeDataNode(Context.User, Reason);
                    int strikes = AdminDataManager.AddStrike(Target, str);

                    if (strikes >= 3) {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"{BotUtils.GetFullUsername(Target)} has 3 strikes"));
                        break;
                    }

                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"Added strike for {BotUtils.GetFullUsername(Target)}. They now have {strikes} strike{((strikes == 1) ? "":"s")}."));

                    if(notifyUser) {
                        bool sent = await BotUtils.DMUserAsync(ServerData.Server.GetUser(Target.Id), new StrikeNotifyEmbed(str.Reason, strikes).GetEmbed());

                        if (!sent) await Context.Channel.SendMessageAsync(BotUtils.BadDMResponse);
                    }

                    break;

                case diamond:
                    // toggles the notification of the user
                    notifyUser = !notifyUser;
                    await UpdateEmbed();
                    return;
            }
             
            EventQueueManager.RemoveMessageEvent(this); // now remove the event
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Add Strike for {BotUtils.GetFullUsername(Target)}");
            eb.WithThumbnailUrl(Target.GetAvatarUrl());
            eb.WithColor(BotUtils.Orange);

            eb.AddField("User's current strike count:", $"{AdminDataManager.GetStrikes(Target)}");

            eb.AddField("Will notify user?", notifyUser ? "Yes":"No");

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
