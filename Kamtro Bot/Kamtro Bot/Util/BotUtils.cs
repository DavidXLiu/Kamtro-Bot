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
        public static bool SaveReady = false; // This is set to true once the files are safe to save to.
        public static bool SaveLoop = true;  // This is set to false to turn off the infinite save loop.

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
                Program.userDataManager.SaveUserData();  // Save the user data.
                Thread.Sleep(new TimeSpan(0, 1, 0));  // Pause for 1 minute
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
    }
}
