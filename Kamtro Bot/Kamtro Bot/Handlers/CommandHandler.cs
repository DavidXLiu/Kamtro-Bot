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
        
        public CommandHandler(DiscordSocketClient c) {
            _client = c;

            _service = new CommandService();
            _provider = new ServiceCollection().AddSingleton(_client).AddSingleton(_service).BuildServiceProvider();
            
            instance = this;

            InstallCommandsAsync();
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
            else
            {
                // Check for other prefixless commands/features - Arcy

                /// MOVE THIS SOMEWHERE ELSE WHEN CLASS IS MADE
                // Check if directed at Kamtro - Arcy
                if (message.Content.Contains(_client.CurrentUser.Id.ToString()) || message.Content.ToLower().Contains("kamtro"))
                {
                    // Check for similar ping command strings - Arcy
                    string[] pingStrings = { "you there", "you up", "you running", "you ok", "you good", };

                    foreach (string s in pingStrings)
                    {
                        if (message.Content.ToLower().Contains(s))
                        {
                            // Check latency and make string - Arcy
                            double localLatency = (DateTime.Now - message.Timestamp.LocalDateTime).Milliseconds;
                            string latencyString = "(Server: " + _client.Latency + " ms | Local: " + localLatency + " ms)";

                            // Respond based on latency - Arcy
                            if (_client.Latency > 500)
                            {
                                await message.Channel.SendMessageAsync("Not always. I'm breaking up. " + latencyString);
                                return;
                            }

                            switch (localLatency)
                            {
                                case double x when (x >= 1000):
                                    await message.Channel.SendMessageAsync("No. I'm not feeling well. I may not respond at times. " + latencyString);
                                    return;
                                case double x when (x >= 500):
                                    await message.Channel.SendMessageAsync("Not really. I'm suffering quite a bit. " + latencyString);
                                    return;
                                case double x when (x >= 300):
                                    await message.Channel.SendMessageAsync("Kinda. I'm being slow. " + latencyString);
                                    return;
                                case double x when (x >= 200):
                                    await message.Channel.SendMessageAsync("Mostly! I'll be a bit slow. " + latencyString);
                                    return;
                                case double x when (x >= 100):
                                    await message.Channel.SendMessageAsync("Yes! I'm doing well! " + latencyString);
                                    return;
                                case double x when (x >= 50):
                                    await message.Channel.SendMessageAsync("Yes! I'm doing great! " + latencyString);
                                    return;
                                case double x when (x >= 10):
                                    await message.Channel.SendMessageAsync("Yeah! I'm perfect right now! " + latencyString);
                                    return;
                                default:
                                    await message.Channel.SendMessageAsync("I don't know... (Error: Latency)");
                                    return;
                            }

                            // Code execution should not continue past this point. - Arcy
                        }
                    }
                }
                /// END
            }
        }
    }
}
