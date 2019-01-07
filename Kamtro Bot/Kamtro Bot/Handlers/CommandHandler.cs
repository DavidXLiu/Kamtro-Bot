using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// This is the class that processes the commands.
    /// </summary>
    public class CommandHandler
    {
        public static CommandHandler instance;

        private DiscordSocketClient _client;
        private CommandService _service;
        private IServiceProvider _provider;
        
        public CommandHandler() {
            _provider = new ServiceCollection().AddSingleton(_client).AddSingleton(this).BuildServiceProvider();  // WARNING: MAY BE BROKEN
            instance = this;
        }

        public async Task HandleCommandAsync(SocketMessage m) {
            if (Program.Settings.prefix == null) return;  // if there is no prefix, also prevents null errors  -C
            if (!(m is SocketUserMessage)) return;  // make sure the message is the appropriate type before casting  -C

            SocketUserMessage message = m as SocketUserMessage; // cast the message -C
            if (message == null) return; // more null checking (You can never be too careful) -C
            if (message.Source != Discord.MessageSource.User) return;  // No bots allowed. #robophobia  -C

            int argPos = 0;  // I never really knew what this did, but it's necessary or everything dies -C
            if(message.HasStringPrefix(Program.Settings.prefix, ref argPos)) {
                // if it's a command  -C
                SocketCommandContext context = new SocketCommandContext(_client, message);
                IResult result = await _service.ExecuteAsync(context, argPos, _provider);

                if(!result.IsSuccess && result.Error != CommandError.UnknownCommand) {
                    // if there was an error with the valid command  -C
                    await message.Channel.SendMessageAsync($"An error occured! {result.ErrorReason}");
                }
            }
        }
    }
}
