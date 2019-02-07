using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces
{
    public class NSFWEmbed : ActionEmbed
    {
        public const string CHECK = "\u2705";
        public const string DECLINE = "\u274C";


        public NSFWEmbed(SocketGuildUser sender) {
            AddMenuOptions(new MenuOptionNode(CHECK, "Accept"),
                new MenuOptionNode(DECLINE, "Decline"));

            CommandCaller = sender;
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("NSFW Rules");
            builder.WithColor(Color.DarkRed);

            builder.AddField("1. NO MEMES", "Suggestive memes can go in #memes-shitposts as long as it's not showing anything. Dark humor is fine.");
            builder.AddField("2. KEEP THINGS CIVIL", "This is an adult chat, if you don't act like one we will remove you without hesitiation.");
            builder.AddField("3. DO NOT RP", "RPing must be taken to PMs if you really need to.");
            builder.AddField("4. WHAT IS IN #mature STAYS IN #mature", "If we find out you've been showing people stuff outside of #mature you will be punished.");
            builder.AddField("5. KEEP NSFW ARTWORK IN THE RIGHT CHANNELS", "Do not post pornography in #mature, you may post your own NSFW artwork in #your-nsfw-creations and other's NSFW artwork in #others-nsfw-creations. Do not post irl pornography.");
            builder.AddField("Lastly", "This channel is meant to be a way to **discuss mature topics without making underage users uncomfortable.** Any intention to post porn may result in __**a blacklist from the channel, strike, or ban depending on the severity of your actions.**__");


            AddMenu(builder);

            return builder.Build();
        }

        public async override Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case CHECK:
                    await option.Channel.SendMessageAsync(BotUtils.KamtroText("You can now see the lewd owo"));
                    ReactionHandler.RemoveEvent(this, option.UserId);

                    await CommandCaller.AddRoleAsync(ServerData.NSFWRole);
                    break;
                case DECLINE:
                    await option.Channel.SendMessageAsync(BotUtils.KamtroText("Understood."));
                    ReactionHandler.RemoveEvent(this, option.UserId);
                    break;
            }
        }
    }
}
