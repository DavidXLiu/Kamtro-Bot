using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class BanEmbed : MessageEmbed
    {
        [InputField("Reason", 1, 1)]
        public string Reason;

        private const string diamond = "\U0001F4A0";

        private SocketUser Target;
        private bool notifyTarget = true;

        public BanEmbed(SocketCommandContext ctx, SocketUser target) {
            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE, new MenuOptionNode(diamond, "Toggle Notify User"));
            Target = target;
            SetCtx(ctx);
            RegisterMenuFields();
        }

        public override async Task PerformAction(SocketReaction action)
        {
            switch (action.Emote.ToString())
            {
                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveMessageEvent(this);
                    await Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText($"The ban on {BotUtils.GetFullUsername(Target)} has been cancelled."));
                    break;

                default:
                    await ButtonAction(action);  // if none of the predefined actions were used, it must be a custom action.
                    break;
            }
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    // Time for security checks

                    /// These checks were done when the command was called - Arcy

                    BanDataNode ban = new BanDataNode(Context.User, Reason);

                    if (notifyTarget) {
                        bool sent = await BotUtils.DMUserAsync(BotUtils.GetGUser(Context), new BanNotifyEmbed(ban.Reason).GetEmbed());

                        if (!sent) await Context.Channel.SendMessageAsync(BotUtils.BadDMResponse);
                    }

                    AdminDataManager.AddBan(Target, ban);
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroAngry($"User {BotUtils.GetFullUsername(Target)} has been banned."));
                        
                    await ServerData.Server.AddBanAsync(Target.Id);
                    break;
                case diamond:
                    notifyTarget = !notifyTarget;
                    await UpdateEmbed();
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Ban {BotUtils.GetFullUsername(Target)}");
            eb.WithThumbnailUrl(Target.GetAvatarUrl());
            eb.WithColor(BotUtils.Red);

            eb.AddField("User Strike Count:", AdminDataManager.GetStrikes(Target).ToString());

            eb.AddField("Will notify user?", notifyTarget ? "Yes" : "No");

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
