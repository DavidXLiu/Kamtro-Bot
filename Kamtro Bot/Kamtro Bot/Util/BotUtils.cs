using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public static bool GCReady = false;
        public static bool GCLoop = true;

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
        /// This method is to optimize saving the user data files.
        /// It will save the file every minute, that way the 
        /// potentially massive file isn't being saved every message or so.
        /// This is not meant to be awaited.
        /// </summary>
        public static void AutoSave() {
            while(SaveReady && SaveLoop) {
                UserDataManager.SaveUserData();  // Save the user data.
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
        /// VERY IMPORTANT: This method has teh potential to cause a race condition with the autosave thread. This is why the two should NEVER
        /// access the same variables.
        /// 
        /// -C
        /// </remarks>
        public static void GarbageCollection() {
            DateTime now;
            TimeSpan span;
            List<EventQueueNode> toRemove = new List<EventQueueNode>();
            List<ulong> toRemoveMsg = new List<ulong>();
            while (GCReady && GCLoop) {
                now = DateTime.Now;

                foreach(KeyValuePair<ulong, MessageEventNode> action in CommandHandler.MessageEventQueue.AsEnumerable()) {
                    span = now - action.Value.TimeCreated;

                    if(span > MessageTimeout) {
                        toRemoveMsg.Add(action.Key);
                    }
                }

                foreach (ulong node in toRemoveMsg) {
                    CommandHandler.MessageEventQueue[node] = null;
                }

                foreach (KeyValuePair<ulong, List<EventQueueNode>> action in ReactionHandler.EventQueue.AsEnumerable()) {
                    foreach(EventQueueNode node in action.Value) {
                        span = now - node.TimeCreated;

                        if (span > Timeout) {
                            toRemove.Add(node);
                        }
                    }

                    foreach (EventQueueNode node in toRemove) {
                        action.Value.Remove(node);
                    }
                }

                Thread.Sleep(new TimeSpan(0, 1, 0));
            }
        }
    }
}
