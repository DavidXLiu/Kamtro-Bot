using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Handlers
{
    /// <summary>
    /// This is the class that processes the commands.
    /// 
    /// Authors: Carbon
    /// </summary>
    public class CommandHandler
    {
        public static CommandHandler instance;

        private DiscordSocketClient _client;
        private CommandService _service;
        private IServiceProvider _provider;
        
        public CommandHandler() {
            _service = new CommandService();
            _provider = new ServiceCollection().AddSingleton(_client).AddSingleton(_service).BuildServiceProvider();
            
            instance = this;
        }

        public async Task InstallCommandsAsync() {
            _client.MessageReceived += HandleCommandAsync;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null); // this is what searches the program for module classes. 
                                                                               // null is passed because the new API is dumb and we aren't using a DI container -C
        }

        public async Task HandleCommandAsync(SocketMessage m) {
            if (Program.Settings.Prefix == null) return;  // if there is no prefix, also prevents null errors  -C
            if (!(m is SocketUserMessage)) return;  // make sure the message is the appropriate type before casting  -C

            SocketUserMessage message = m as SocketUserMessage; // cast the message -C
            if (message == null) return; // more null checking (You can never be too careful) -C
            if (message.Source != Discord.MessageSource.User) return;  // No bots allowed. #robophobia  -C

            int argPos = 0;  // This is the position of the command character, it should usually be 0. -C
            if(message.HasStringPrefix(Program.Settings.Prefix, ref argPos)) {
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
