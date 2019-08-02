﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Nodes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Kamtro_Bot.Util;
using Kamtro_Bot.Managers;

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

            #region Message Relay
            if (m.Channel is SocketDMChannel && !m.Author.IsBot)
            {
                // Relay to each user
                foreach (SocketGuildUser user in ServerData.RelayUsers)
                {
                    // Make sure bot is able to send to user
                    if (!user.GetOrCreateDMChannelAsync().IsFaulted)
                    {
                        // Check for attachments
                        if (m.Attachments.Count > 0)
                        {
                            await user.SendMessageAsync($"{m.Author.Username}#{m.Author.Discriminator}: {m.Attachments.ToList()[0].Url}\n{m.Content}");
                        }
                        else
                        {
                            await user.SendMessageAsync($"{m.Author.Username}#{m.Author.Discriminator}: {m.Content}");
                        }
                    }
                }
            }
            #endregion

            if (!(m is SocketUserMessage)) return;  // make sure the message is the appropriate type before casting  -C

            SocketUserMessage message = m as SocketUserMessage; // cast the message -C
            if (message == null) return; // more null checking (You can never be too careful) -C

            SecurityManager.CheckMessage(message);  // Security Clearance -C

            if (Program.Settings.Prefix == null) return;  // if there is no prefix, also prevents null errors  -C
            if (message.Source != MessageSource.User) return;  // No bots allowed. #robophobia  -C

            // From here on, only valid commands will get past
            int argPos = 0;  // This is the position of the command character, it should usually be 0. -C
            if(message.HasStringPrefix(Program.Settings.Prefix, ref argPos)) {
                // if it's a command  -C
                SocketCommandContext context = new SocketCommandContext(_client, message);
                IResult result = await _service.ExecuteAsync(context, argPos, _provider);

                if(!result.IsSuccess && result.Error != CommandError.UnknownCommand) {
                    // if there was an error with the valid command  -C
                    await message.Channel.SendMessageAsync($"An error occured! {result.ErrorReason}");
                    //Console.WriteLine(result.Error);
                }
            } else {
                // Check for other prefixless commands/features - Arcy

                // Check if the user is responding to an interface. -C
                // If the user has an entry in the dict, and their entry isn't null. -C
                if(EventQueueManager.MessageEventQueue.ContainsKey(m.Author.Id) && EventQueueManager.MessageEventQueue[m.Author.Id] != null) {
                    SocketUserMessage sm = m as SocketUserMessage;
                    if(sm != null) {
                        await EventQueueManager.MessageEventQueue[m.Author.Id].Interface.PerformMessageAction(sm);
                        return;
                    }
                }

                SecurityManager.CheckMessage(message);
                UserDataManager.OnChannelMessage(message);  // evaluate the message for user score
                await GeneralHandler.HandleMessage(message);

                /// MOVE THIS SOMEWHERE ELSE WHEN CLASS IS MADE
                // Check if directed at Kamtro - Arcy
                if (message.Content.Contains(_client.CurrentUser.Id.ToString()) || message.Content.ToLower().Contains("kamtro"))
                {
                    // Check for similar ping command strings - Arcy
                    string[] pingStrings = { "you there", "you up", "you running", "you ok", "you good", "you doing ok" };

                    foreach (string s in pingStrings)
                    {
                        if (message.Content.ToLower().Contains(s)) {
                            // Check latency and make string - Arcy
                            double localLatency = (DateTime.Now - message.Timestamp.LocalDateTime).Milliseconds;
                            string latencyString = "(Server: " + _client.Latency + " ms | Local: " + localLatency + " ms)";

                            // Respond based on latency - Arcy
                            if (_client.Latency > 500)
                            {
                                await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Not always. I'm breaking up. " + latencyString));
                                return;
                            }

                            switch (localLatency) {
                                case double x when (x >= 1000):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"No. I'm not feeling well. I may not respond at times. " + latencyString));
                                    return;
                                case double x when (x >= 500):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Not really. I'm suffering quite a bit. " + latencyString));
                                    return;
                                case double x when (x >= 300):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Kinda. I'm being slow. " + latencyString));
                                    return;
                                case double x when (x >= 200):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Mostly! I'll be a bit slow. " + latencyString));
                                    return;
                                case double x when (x >= 100):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Yes! I'm doing well! " + latencyString));
                                    return;
                                case double x when (x >= 50):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Yes! I'm doing great! " + latencyString));
                                    return;
                                case double x when (x >= 10):
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Yeah! I'm perfect right now! " + latencyString));
                                    return;
                                default:
                                    await message.Channel.SendMessageAsync(BotUtils.KamtroText($"I don't know... (Error: Latency)"));
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
