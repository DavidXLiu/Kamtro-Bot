using Discord;
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
using Kamtro_Bot.Interfaces.BasicEmbeds;

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
            if (!Program.Ready) return;  // wait until program is ready
            #region Message Relay
            if (m.Channel is SocketDMChannel && !m.Author.IsBot)
            {
                // Relay to each user
                foreach (SocketGuildUser user in ServerData.RelayUsers)
                {
                    // Make sure bot is able to send to user and don't send same user messages
                    if (!user.GetOrCreateDMChannelAsync().IsFaulted && m.Author.Id != user.Id)
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
            //////////////////////////////////////
            /// Command Checks Here
            //////////////////////////////////////
            if (message.HasStringPrefix(Program.Settings.Prefix, ref argPos) && (message.Channel.Id == Program.Settings.BotChannelID || message.Channel.Id == Program.Settings.AdminChannelID || (ServerData.Server.GetUser(message.Author.Id) != null && ServerData.HasPermissionLevel(ServerData.Server.GetUser(message.Author.Id), ServerData.PermissionLevel.ADMIN)) || (message.Channel is IPrivateChannel && ServerData.Server.GetUser(message.Author.Id) != null))) {                
                // if it's a command in the right channel or a DM  -C
                SocketCommandContext context = new SocketCommandContext(_client, message);
                IResult result = await _service.ExecuteAsync(context, argPos, _provider);

                if(!result.IsSuccess && result.Error != CommandError.UnknownCommand) {
                    // if there was an error with the valid command  -C
                    
                    switch(result.Error) {
                        case CommandError.BadArgCount:
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText("Your parameters were invalid, and no special case was handled for this command. Please ping Arcy or Carbon, or DM one of them with a screenshot of this. (Make sure you get the command you used in the screenshot!)"));
                            break;

                        case CommandError.Exception:
                            if(result is ExecuteResult execRes) {
                                string st = execRes.Exception.StackTrace.Substring(0, execRes.Exception.StackTrace.IndexOf("---")).Trim('\n', '\r', ' ');

                                await message.Channel.SendMessageAsync(BotUtils.KamtroText($"Something went wrong in that command! Please ping Arcy or Carbon.\n\nException:\n{execRes.Exception.GetType().ToString()}\nReason:\n{execRes.ErrorReason}\nStack Trace:\n{st}"));
                                // ErrorReportEmbed er = new ErrorReportEmbed(execRes.Exception);
                                // await er.Display(message.Channel);
                            }
                            break;

                        default:
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText($"An error occured! {result.ErrorReason}"));
                            break;
                    }
                }
            } else {
                // Check for other prefixless commands/features - Arcy

                // Auto-delete commands used in other channel

                // Check if the user is responding to an interface. -C
                // If the user has an entry in the dict, and their entry isn't null. -C
                if(EventQueueManager.MessageEventQueue.ContainsKey(m.Author.Id) && EventQueueManager.MessageEventQueue[m.Author.Id] != null) {
                    SocketUserMessage sm = m as SocketUserMessage;
                    if(sm != null) {
                        await EventQueueManager.MessageEventQueue[m.Author.Id].Interface.PerformMessageAction(sm);
                        return;
                    }
                }

                SecurityManager.CheckMessage(message);  // tba
                await GeneralHandler.HandleMessage(message); // Evaluate for consecutive messages and autoban
                UserDataManager.OnChannelMessage(message);  // evaluate the message for user score

                /// MOVE THIS SOMEWHERE ELSE WHEN CLASS IS MADE
                // Check if directed at Kamtro - Arcy
                if ((message.Channel.Id == Program.Settings.BotChannelID || message is IDMChannel)
                    && (message.Content.Contains(_client.CurrentUser.Id.ToString()) || message.Content.ToLower().StartsWith("kamtro") || message.Content.ToLower().EndsWith("kamtro")))
                {
                    /// ORDER THESE CHECKS FROM LEAST TIME TO MOST TIME

                    #region Question Ask
                    /// Question Ask is a feature for users to get a random response to their question.
                    /// These responses are usually in the forms of yes, no, or maybe.
                    /// Author: Arcy
                    if (message.Content.EndsWith("?"))
                    {
                        // Array of all the responses
                        string[] responseStrings = {
                            // Yes
                            "Of course.", "Beep Boop. My algorithms predict that is probably true.", "Yes...?", "Could be.",
                            "Most likely.", "Tomorrow it will be.", "Fortunately, yes.", "Yeah!", "Yep.", "Certainly.",
                            "Yes but not always.", "Totally.",

                            // No
                            "No.", "Not really.", "Unlikely.", "Yeah, no.", "Absolutely not.", "No way!", "Not a chance.",
                            "Nah.",

                            // Maybe
                            "Possibly.", "Maybe.", "It's probably better not to answer.", "Only if you believe in it.",
                            "Flip a coin to find out.", "If there is a will, there is a way.", "Are you sure you want to know?",

                            // Jokes
                            "Only on Tuesdays.", "What was the question again?", "o3o", "I do not know. I am just a robotic dragon.",
                            "Idk ask Arcy.", "Only after a trip to Olive Garden.", "I should be asking you that question.",
                            "I am a bot. I cannot tell.", "...", "Maybe someday.", "Error 404: Response not found.",
                            "Let me think about that.", "Soon."
                        };
                        Random rnd2 = new Random();

                        // Send a random response from the array
                        await message.Channel.SendMessageAsync(BotUtils.KamtroText(responseStrings[rnd2.Next(0, responseStrings.Length)]));
                        return;
                    }
                    #endregion

                    #region Ping Ask
                    /// Ping Ask is a feature for users to check the latency on Kamtro.
                    /// Depending on certain strings, this should detect if a user is asking Kamtro if they are running well.
                    /// Author: Arcy
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
                    #endregion

                    #region Thanks
                    /// Thanks is a simple feature where Kamtro will respond with different ways of saying "Your Welcome".
                    /// This will check for popular langauges.
                    /// Author: Arcy
                    // Check for thanks in common languages - Arcy
                    string[] thanksStringsEn = { "thank" };
                    string[] thanksStringsEs = { "gracias", "obrigado" };
                    string[] thanksStringsDe = { "danke", "vielen dank" };
                    string[] thanksStringsJp = { "arigato", "ありがとう", "doumo", "どうも" };
                    string[] thanksStringsCn = { "xiè", "谢" };
                    string[] thanksStringsBinary = { "0101010001101000011000010110111001101011", "0111010001101000011000010110111001101011" };

                    string[] welcomeStringsEn = { "You're welcome!", "No problem!", "Anytime!" };
                    string[] welcomeStringsEs = { "De nada!", "Mucho gusto!" };
                    string[] welcomeStringsDe = { "Bitte!", "Kein problem!" };
                    string[] welcomeStringsJp = { "こちらこそ、ありがとうございます。", "いえいえ。" };
                    string[] welcomeStringsCn = { "不客气", "你太客气了" };
                    string[] welcomeStringsBinary = { "010110010110111101110101001001110111001001100101001000000111011101100101011011000110001101101111011011010110010100100001" };
                    Random rnd = new Random();

                    foreach (string s in thanksStringsEn)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsEn[rnd.Next(0, welcomeStringsEn.Length)]));
                    }
                    foreach (string s in thanksStringsEs)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsEs[rnd.Next(0, welcomeStringsEs.Length)]));
                    }
                    foreach (string s in thanksStringsDe)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsDe[rnd.Next(0, welcomeStringsDe.Length)]));
                    }
                    foreach (string s in thanksStringsJp)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsJp[rnd.Next(0, welcomeStringsJp.Length)]));
                    }
                    foreach (string s in thanksStringsCn)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsCn[rnd.Next(0, welcomeStringsCn.Length)]));
                    }
                    foreach (string s in thanksStringsBinary)
                    {
                        if (message.Content.ToLower().Contains(s))
                            await message.Channel.SendMessageAsync(BotUtils.KamtroText(welcomeStringsBinary[rnd.Next(0, welcomeStringsBinary.Length)]));
                    }
                    #endregion
                }
                /// END
            }
        }
    }
}
