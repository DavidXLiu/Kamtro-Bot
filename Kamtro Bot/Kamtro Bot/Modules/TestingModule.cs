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
        [Command("ping")]
        [Name("Ping")]
        [Summary("Simple ping command, bot responds with pong.")]
        public async Task PingAsync() {
            double localLatency = (DateTime.Now - Context.Message.Timestamp.LocalDateTime).Milliseconds;
            await ReplyAsync("Pong! (Server: " + Context.Client.Latency + " ms | Local: " + localLatency + " ms)");
        }
    }
}
