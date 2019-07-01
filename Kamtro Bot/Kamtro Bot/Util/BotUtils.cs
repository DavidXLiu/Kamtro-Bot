using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Discord.WebSocket;

namespace Kamtro_Bot
{
    /// <summary>
    /// General utility functions for Kamtro
    /// -C
    /// </summary>
    public class BotUtils
    {
        public static readonly TimeSpan Timeout = new TimeSpan(0, 10, 0);
        public static readonly TimeSpan MessageTimeout = new TimeSpan(0, 1, 0);

        public static bool SaveReady = false; // This is set to true once the files are safe to save to.
        public static bool SaveLoop = true;  // This is set to false to turn off the infinite save loop.
        public static bool PauseSave = false;
        public static bool SaveInProgress = false;

        public static bool GCReady = false;
        public static bool GCLoop = true;
        public static bool PauseGC = false;
        public static bool GCInProgress = false;

        /// <summary>
        /// Formats the message so the text is blue (AKA Kamtro speak)
        /// -C
        /// </summary>
        /// <param name="message">The message you want to put into kamtro-formatting</param>
        /// <returns>The message in kamtro-formatting</returns>
        public static string KamtroText(string message) {
            return $"```INI\n[{message}]\n```";
        }

        /// <summary>
        /// This is a helper method to save an object to JSON given the location and object.
        /// </summary>
        /// <param name="location">The path of the file it will be saved to.</param>
        /// <param name="toWrite">The object to be saved.</param>
        public static void WriteToJson(object toWrite, string location) {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(location)) {
                using (JsonTextWriter writer = new JsonTextWriter(sw)) {
                    writer.Formatting = Formatting.Indented; // make it so that the entire file isn't on one line
                    serializer.Serialize(writer, toWrite);  // serialize the settings object and save it to the file
                }
            }
        }

        /// <summary>
        /// Gets the user's name and discriminator in standard form (Username#XXXX)
        /// </summary>
        /// <param name="user">The user to inspect</param>
        /// <returns>The user's full name and discriminator</returns>
        public static string GetFullUsername(SocketUser user) {
            return user.Username + "#" + user.Discriminator;
        }

        /// <summary>
        /// This method is to optimize saving the user data files.
        /// It will save the file every minute, that way the 
        /// potentially massive file isn't being saved every message or so.
        /// This is not meant to be awaited.
        /// </summary>
        public static void AutoSave() {
            while (SaveReady && SaveLoop) {
                if (!PauseSave) {
                    SaveInProgress = true;
                    UserDataManager.SaveUserData();  // Save the user data, but only if the thread is not paused.
                    SaveInProgress = false;
                }

                Thread.Sleep(new TimeSpan(0, 10, 0));  // Pause for 10 minutes
            }
        }

        public static void StartAutosaveLoop() {
            SaveLoop = true;  // Turn on the loop
            Program.Autosave.Start();  // Start the thread again
        }

        public static void StopAutosaveLoop() {
            SaveLoop = false;  // Stop the loop.
            Program.Autosave.Abort();  // Stop the thread here, just in case
        }

        /// <summary>
        /// Garbage collection thread method.
        /// This stops kamtro from waiting for interfaces that are going to be unused.
        /// </summary>
        /// <remarks>
        /// VERY IMPORTANT: This method has the potential to cause a race condition with the autosave thread. This is why the two should NEVER
        /// access the same variables.
        /// 
        /// -C
        /// </remarks>
        public static void GarbageCollection() {
            DateTime now;
            TimeSpan span;

            while (GCReady && GCLoop) {
                if(PauseGC) {
                    Thread.Sleep(new TimeSpan(0, 1, 0));
                    continue;
                }

                GCInProgress = true;

                List<EventQueueNode> toRemove = new List<EventQueueNode>();
                List<ulong> toRemoveMsg = new List<ulong>();
                now = DateTime.Now;

                foreach (KeyValuePair<ulong, MessageEventNode> action in EventQueueManager.MessageEventQueue.AsEnumerable()) {
                    span = now - action.Value.TimeCreated; 

                    if (span > MessageTimeout) {
                        toRemoveMsg.Add(action.Key);
                    }
                }

                foreach (ulong node in toRemoveMsg) {
                    EventQueueManager.RemoveMessageEvent(node);
                    Console.WriteLine($"[GC] Message Event from user {node} removed.");
                }

                foreach (KeyValuePair<ulong, List<EventQueueNode>> action in EventQueueManager.EventQueue.AsEnumerable()) {
                    foreach (EventQueueNode node in action.Value) {
                        span = now - node.TimeCreated;

                        if (span > Timeout || node.IsAlone && span > MessageTimeout) {
                            toRemove.Add(node);
                        }
                    }

                    foreach (EventQueueNode node in toRemove) {
                        action.Value.Remove(node);
                        Console.WriteLine($"[GC] Event from user {action.Key} removed. {((node.IsAlone) ? "Event was paired with message event from same user.":"")}");
                    }
                }

                GCInProgress = false;

                Thread.Sleep(new TimeSpan(0, 1, 0));
            }
        }

        /// <summary>
        /// Finds the <see cref="SocketGuildUser"/> in the given message and returns it in the list. If a user cannot be distinguished by the given message, a list of users are put out containing the possible users.
        /// <para></para>
        /// The <see cref="string"/> command finds and removes that <see cref="string"/> out of the <see cref="SocketMessage"/> content.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>User found from the message.</returns>
        /// Arcy
        public static List<SocketGuildUser> GetUser(SocketMessage message, string command = "") {
            List<SocketGuildUser> users = new List<SocketGuildUser>();

            // Find mentions
            if (message.MentionedUsers.Count > 0) {
                foreach (SocketUser user in message.MentionedUsers.ToList()) {
                    users.Add(ServerData.Server.GetUser(user.Id));
                }

                return users;
            }

            string remainder = message.Content.Substring(message.Content.IndexOf(' ')  + 1).ToLower();  // Gets everything after the command separated by a space

            foreach (SocketGuildUser user in ServerData.Server.Users) {
                // Add to list if username or nickname contains the name, or if it contains user ID
                if (user.Username.ToLower().Contains(remainder)) {
                    users.Add(user);
                } else if (user.Nickname != null && user.Nickname.Contains(remainder)) {
                    users.Add(user);
                } else if (remainder.Contains(user.Id.ToString())) {
                    users.Add(user);
                }
            }

            return users;
        }

        /// <summary>
        /// Finds the <see cref="SocketGuildUser"/> in the given <see cref="string"/> text and returns it in the list.
        /// <para></para>
        /// This method returns the best available option that has a word score of above .66. Otherwise, the method returns <see cref="null"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>User found from the message.</returns>
        /// Arcy
        public static SocketGuildUser GetUser(string text)
        {
            // Check through all users and determine the best user found
            SocketGuildUser bestUser = null;
            float bestScore = 0;

            foreach (SocketGuildUser user in ServerData.Server.Users)
            {
                // Check if username or nickname contains the name, or if it contains user ID
                if (user.Username.ToLower().Contains(text.ToLower()))
                {
                    float score = UtilStringComparison.CompareWordScore(user.Username.ToLower(), text.ToLower());
                    if (score > bestScore)
                    {
                        bestUser = user;
                        bestScore = score;
                        score = 0;
                    }
                }
                if (user.Nickname != null && user.Nickname.Contains(text.ToLower()))
                {
                    float score = UtilStringComparison.CompareWordScore(user.Nickname.ToLower(), text.ToLower());
                    if (score > bestScore)
                    {
                        bestUser = user;
                        bestScore = score;
                        score = 0;
                    }
                }
                // User ID most likely would not be put in by mistake. Return that user
                if (text.Contains(user.Id.ToString()))
                {
                    return user;
                }
            }

            // Check if match is reasonably close
            if (bestScore > 0.66f)
            {
                return bestUser;
            }
            else
            {
                return null;
            }
        }
    }
}
