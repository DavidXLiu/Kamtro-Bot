using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces
{
    public class HackedEmbed : ActionEmbed
    {
        public HackedEmbed(SocketGuildUser sender) {
            CommandCaller = sender;

            AddMenuOptions(new MenuOptionNode(BotUtils.CHECK, "Confirm"), new MenuOptionNode(BotUtils.DECLINE, "Cancel"));
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("HACKED");
            builder.WithColor(Color.Red);

            builder.WithDescription("Are you sure?");

            AddMenu(builder);

            return builder.Build();
        }

        public async override Task PerformAction(SocketReaction option) {
            if(option.Emote.ToString() == BotUtils.CHECK) {
                await Hacked();
            }

            ReactionHandler.RemoveEvent(this, CommandCaller.Id);
        }

        private async Task Hacked() {
            // leave all servers
            Console.WriteLine("Bot has been hacked! Leaving all servers...");

            foreach (SocketGuild server in Program.Client.Guilds) {
                await server.LeaveAsync();

                Console.WriteLine($"Left {server.Name} [ID: {server.Id}]");
            }
        }
    }
}
