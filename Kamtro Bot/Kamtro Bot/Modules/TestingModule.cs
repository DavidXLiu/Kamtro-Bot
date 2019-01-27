using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This module will contain some testing commands
    /// </summary>
    [Name("Testing")]
    public class TestingModule : ModuleBase<SocketCommandContext>
    {
        #region Ping Commands
        [Command("ping")]
        [Name("Ping")]
        [Summary("Simple ping command, bot responds with pong and latency.")]
        public async Task PingAsync() {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Pong! (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("pong")]
        [Name("Pong")]
        [Summary("Fun alternative to the ping command. Bot responds with ping and latency.")]
        public async Task PongAsync()
        {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Ping? (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("marco")]
        [Name("Marco")]
        [Summary("Another fun alternative to the ping command. Bot responds with ping and latency.")]
        public async Task MarcoAsync()
        {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync(BotUtils.KamtroText($"Polo! (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)"));
        }

        [Command("polo")]
        [Name("Polo")]
        [Summary("Fun and silly response in a user's expectations for a Polo command.")]
        public async Task PoloAsync()
        {
            await ReplyAsync(BotUtils.KamtroText($"But nobody came."));
        }
        #endregion
    }
}
