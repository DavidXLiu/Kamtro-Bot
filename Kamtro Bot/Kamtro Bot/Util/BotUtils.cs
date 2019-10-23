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
using Discord;
using Discord.Commands;

namespace Kamtro_Bot
{
    /// <summary>
    /// General utility functions for Kamtro
    /// -C
    /// </summary>
    public class BotUtils
    {
        public const string ZeroSpace = "​"; // This is a zero-width space. (it's invisible)  -C

        public static readonly Color Red = new Color(247, 47, 60);
        public static readonly Color Orange = new Color(250, 148, 80);
        public static readonly Color Yellow = new Color(245, 245, 66);
        public static readonly Color Green = new Color(97, 237, 116);
        public static readonly Color Blue = new Color(97, 193, 237);
        public static readonly Color Purple = new Color(146, 97, 237);
        public static readonly Color PurpleMagenta = new Color(176, 97, 237);
        public static readonly Color BrightMagenta = new Color(225, 97, 237);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Grey = new Color(185, 200, 203);
        public static readonly Color Kamtro = new Color(137, 232, 249);

        public static DateTime LastWeeklyReset = DateTime.Now.LastSunday();

        public static bool SaveReady = false; // This is set to true once the files are safe to save to.  -C
        public static bool SaveLoop = true;  // This is set to false to turn off the infinite save loop.  -C
        public static bool PauseSave = false;
        public static bool SaveInProgress = false;

        public static bool GCReady = false;
        public static bool GCLoop = true;
        public static bool PauseGC = false;
        public static bool GCInProgress = false;

        public static string BadDMResponse = KamtroText("I tried to send the user a message, but they had me blocked and/or disabled direct messages from server members, so I couldn't message them!");

        #region Text Utils
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
        /// Formats the message to be red.
        /// </summary>
        /// <param name="message"The message</param>
        /// <returns>The formatted message</returns>
        public static string KamtroAngry(string message) {
            return $"```DIFF\n- {message} -\n```";
        }

        /// <summary>
        /// Sends the specified message to the selected admin channel.
        /// </summary>
        /// <param name="text">The text to send</param>
        /// <param name="angry">True if the text should be red, false for normal kamtro blue.</param>
        /// <returns></returns>
        public static async Task AdminLog(string text, bool angry = true) {
            if (angry) {
                await ServerData.AdminChannel.SendMessageAsync(KamtroAngry(text));
            } else {
                await ServerData.AdminChannel.SendMessageAsync(KamtroText(text));
            }
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
        /// This method gets the effective extension of a file.
        /// </summary>
        /// <remarks>
        /// The active extension is the text after the LAST . in the name
        /// For example, test.xlsx would have an extension of xlsx.
        /// myFile.tar.gz would have an extension of gz.
        /// </remarks>
        /// <param name="filename">The name of the file</param>
        /// <returns>The file's extension</returns>
        public static string GetFileExtension(string filename) {
            if (!filename.Contains(".") || filename.LastIndexOf('.') + 1 == filename.Length) return "";

            string ext = filename.Substring(filename.LastIndexOf('.') + 1);
            return ext;
        }
        #endregion
        #region Threads
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

                Thread.Sleep(new TimeSpan(0, 1, 0));  // Pause for 1 minute
            }
        }

        /// <summary>
        /// Starts the Autosave loop
        /// </summary>
        public static void StartAutosaveLoop() {
            SaveLoop = true;  // Turn on the loop
            Program.Autosave.Start();  // Start the thread again
        }

        /// <summary>
        /// Stops the autosave loop
        /// </summary>
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
                if (PauseGC) {
                    Thread.Sleep(new TimeSpan(0, 1, 0));
                    continue;
                }

                GCInProgress = true;

                List<EventQueueNode> toRemove = new List<EventQueueNode>();
                List<ulong> toRemoveMsg = new List<ulong>();
                now = DateTime.Now;

                foreach (KeyValuePair<ulong, MessageEventNode> action in EventQueueManager.MessageEventQueue.AsEnumerable()) {
                    span = now - action.Value.TimeCreated;

                    if (span > action.Value.Interface.Timeout) {
                        toRemoveMsg.Add(action.Key);

                        // Let user know embed is disabled
                        action.Value.Interface.Message.ModifyAsync(x => x.Content = KamtroText("This embed is no longer in use."));
                        action.Value.Interface.Message.ModifyAsync(x => x.Embed = null);
                    }
                }

                foreach (ulong node in toRemoveMsg) {
                    EventQueueManager.RemoveMessageEvent(node);
                    Console.WriteLine($"[GC] Message Event from user {node} removed.");
                }

                foreach (KeyValuePair<ulong, List<EventQueueNode>> action in EventQueueManager.EventQueue.AsEnumerable()) {
                    foreach (EventQueueNode node in action.Value) {
                        span = now - node.TimeCreated;

                        if (span > node.EventAction.Timeout || node.IsAlone && span > node.EventAction.Timeout) {
                            toRemove.Add(node);

                            // Let user know embed is disabled
                            node.EventAction.Message.ModifyAsync(x => x.Content = KamtroText("This embed is no longer in use."));
                            node.EventAction.Message.ModifyAsync(x => x.Embed = null);
                        }
                    }

                    foreach (EventQueueNode node in toRemove) {
                        action.Value.Remove(node);
                        Console.WriteLine($"[GC] Event from user {action.Key} removed. {((node.IsAlone) ? "Event was paired with message event from same user." : "")}");
                    }
                }

                GCInProgress = false;

                Thread.Sleep(new TimeSpan(0, 1, 0));
            }
        }

        /// <summary>
        /// Thread that handles all events that depend on the date
        /// </summary>
        public static void DateEventCheckLoop() {
            while (true) {
                // Check for weekly reset
                if (DateTime.Now - LastWeeklyReset > new TimeSpan(7, 0, 0, 0)) {
                    // reset things
                    UserDataManager.ResetWeekly();
                    UserDataManager.ResetRep();

                    // set the new time
                    LastWeeklyReset = DateTime.Now.LastSunday();

                    File.WriteAllText(DataFileNames.LastDateFile, LastWeeklyReset.Ticks.ToString());
                }

                KLog.Debug($"Last Weekly Reset: [{LastWeeklyReset.ToString("F")}]");

                Thread.Sleep(new TimeSpan(0, 20, 0));
            }
        }
        #endregion
        #region Server Utils
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
                    users.Add(BotUtils.GetGUser(user.Id));
                }

                return users;
            }

            string remainder = message.Content.Substring(message.Content.IndexOf(' ') + 1).ToLower();  // Gets everything after the command separated by a space

            foreach (SocketGuildUser user in ServerData.Server.Users) {
                // Add to list if username or nickname contains the name, or if it contains user ID
                if (user.Username.ToLower().Contains(remainder))
                    users.Add(user);
                else if (user.Nickname != null && user.Nickname.ToLower().Contains(remainder))
                    users.Add(user);
                else if (remainder == user.Username.ToLower() + "#" + user.Discriminator)
                    users.Add(user);
                else if (remainder.Contains(user.Id.ToString()))
                    users.Add(user);
            }

            return users;
        }

        public static List<SocketGuildUser> GetUsers(string name) {
            List<SocketGuildUser> users = new List<SocketGuildUser>();

            name = name.ToLower();

            foreach (SocketGuildUser user in ServerData.Server.Users) {
                // Add to list if username or nickname contains the name, or if it contains user ID
                if (user.Username.ToLower().Contains(name))
                    users.Add(user);
                else if (user.Nickname != null && user.Nickname.ToLower().Contains(name))
                    users.Add(user);
                else if (name == user.Username.ToLower() + "#" + user.Discriminator)
                    users.Add(user);
                else if (name.Contains(user.Id.ToString()))
                    users.Add(user);
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
        public static SocketGuildUser GetUser(string text, float threshold = 0.5f) {
            // Check through all users and determine the best user found
            SocketGuildUser bestUser = null;
            float bestScore = 0;

            foreach (SocketGuildUser user in ServerData.Server.Users) {
                // Check if username or nickname contains the name, or if it contains user ID
                if (user.Username.ToLower().Contains(text.ToLower())) {
                    float score = UtilStringComparison.CompareWordScore(user.Username.ToLower(), text.ToLower());
                    if (score > bestScore) {
                        bestUser = user;
                        bestScore = score;
                        score = 0;
                    }
                }
                if (user.Nickname != null && user.Nickname.Contains(text.ToLower())) {
                    float score = UtilStringComparison.CompareWordScore(user.Nickname.ToLower(), text.ToLower());
                    if (score > bestScore) {
                        bestUser = user;
                        bestScore = score;
                        score = 0;
                    }
                }
                // User ID most likely would not be put in by mistake. Return that user
                if (text.Contains(user.Id.ToString())) {
                    return user;
                }
            }

            // Check if match is reasonably close
            if (bestScore > threshold) {
                return bestUser;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Checks to see if A is higher in role structure than B
        /// </summary>
        /// <param name="a">The first user</param>
        /// <param name="b">The user to compare</param>
        /// <returns>True if A is higher up than B, false if A is equal to, or lower than B.</returns>
        public static bool HighestUser(SocketGuildUser a, SocketGuildUser b, bool orEqual = false) {
            if (orEqual)
                return a.Hierarchy >= b.Hierarchy;
            else
                return a.Hierarchy > b.Hierarchy;
        }

        /// <summary>
        /// Attempts to send the specified message to the specified user.
        /// </summary>
        /// <param name="user">The user to send the message to</param>
        /// <param name="e">The embed to send</param>
        /// <param name="msg">Text to be included in the message</param>
        /// <returns>True if the message was successfully sent, false otherwise</returns>
        public static async Task<bool> DMUserAsync(SocketGuildUser user, Embed e = null, string msg = "") {
            // I'm not sure what this check is for since this method is used for more than just titles, but it interferes so I'm
            // commenting it out for now - Arcy
            //if (!UserDataManager.GetUserSettings(user).TitleNotify) return true;

            try {
                await user.SendMessageAsync(msg, false, e);  // try to send the message
                return true; // if that works, things are fine
            } catch (Exception) {
                return false; // otherwise if it fails, say so.
            }
        }

        public static SocketGuildUser GetGUser(SocketCommandContext ctx) {
            return BotUtils.GetGUser(ctx.User.Id);
        }

        public static SocketGuildUser GetGUser(ulong id) {
            return ServerData.Server.GetUser(id);
        }

        /// <summary>
        /// Gets a list of roles whose names contain the specified text
        /// </summary>
        /// <param name="name">The name of the role</param>
        /// <returns>The list of roles</returns>
        public static List<SocketRole> GetRoles(string name) {
            List<SocketRole> tmp = new List<SocketRole>();

            foreach (SocketRole role in ServerData.Server.Roles) {
                if (role.Name.ToLower().Contains(name.ToLower())) tmp.Add(role);
            }

            return tmp;
        }

        public static SocketRole GetRole(ulong id) {
            foreach (SocketRole role in ServerData.Server.Roles) {
                if (role.Id == id) {
                    return role;
                }
            }

            return null;
        }
        #endregion
    }

    #region Extension Methods
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class GeneralExtensions
    {
        // DateTime
        public static DateTime LastSunday(this DateTime dt) {


            int diff = dt.DayOfWeek - DayOfWeek.Sunday;


            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets the name that should be displayed in an embed, which is the username if there is no nickname already available
        /// </summary>
        /// <param name="user"></param>
        /// <param name="desc">Whether or not to include the descriminator in the case of no nickname. Default is false.</param>
        /// <returns>The display name of the user.</returns>
        public static string GetDisplayName(this SocketGuildUser user, bool desc = false) {
            // If user.Nickname is not null, return it. Otherwise, return the username optionally with descriminator
            return user.Nickname ?? (desc ? BotUtils.GetFullUsername(user) : user.Username);
        }

        public static bool HasRole(this SocketGuildUser user, SocketRole role) {
            return user.Roles.Contains(role);
        }
    }
    #endregion
}


