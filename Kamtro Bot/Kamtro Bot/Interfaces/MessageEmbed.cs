using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;
using Kamtro_Bot.Util.Exceptions;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// This is a base class for embeds that are waiting on a message.
    /// </summary>
    /// <remarks>
    /// Class chain so far is:
    /// 
    /// KamtroEmbedBase > ActionEmbed > MessageEmbed
    /// </remarks>
    /// -C
    public abstract class MessageEmbed : ActionEmbed
    {
        public SocketChannel CommandChannel;

        // Storage for the input fields -C
        public Dictionary<int, Dictionary<int, MessageFieldNode>> InputFields = new Dictionary<int, Dictionary<int, MessageFieldNode>>();

        public int FieldCount = 0;
        public int PageCount = 0;
        public int PageNum = 1;
        public int CursorPos = 1;

        public abstract Task OnMessage(SocketUserMessage message);

        /// <summary>
        /// This is the method that will be called when the user sends a message in the bot channel if the interface is waiting on a message.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that the message is from the correct user in the correct channel when this is called.
        /// </remarks>
        /// -C
        /// <param name="message">The message that was sent by the user</param>
        public virtual void PerformMessageAction(SocketUserMessage message) {
            InputFields[PageNum][CursorPos].SetValue(message.Content);
        }
        

        /// <summary>
        /// Registers the menu fields.
        /// </summary>
        /// <remarks>
        /// Important to note:
        /// Be careful with how you arrange these fields.
        /// If a field is in a spot that is already occupied, 
        /// It will throw an error. 
        /// </remarks>
        /// -C
        protected void RegisterMenuFields() {
            foreach(PropertyInfo f in GetType().GetProperties()) {
                if(Attribute.IsDefined(f, typeof(InputField))) {
                    // if it's an input field var
                    InputField info = f.GetCustomAttribute(typeof(InputField)) as InputField; // get it's attribute

                    int page = info.Page;
                    int pos = info.Position;

                    if(InputFields[page].ContainsKey(pos)) {
                        // If there is already an input field here
                        throw new ConflictingFieldException(InputFields[page][pos].Name, info.Name, page, pos); // Throw an error.
                    }

                    // Otherwise, if everything is fine,
                    // Add the field.
                    InputFields[info.Page][info.Position] = new MessageFieldNode(info.Name, info.Page, info.Position, info.Value);  // And add the appropriate node to the dict where it belongs

                    FieldCount++; // Account for the new field
                }
            }

            PageCount = InputFields.Values.Count(); // set the number of pages
        }

        /// <summary>
        /// Adds the embed fields.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -C
        /// <param name="builder">The embed builder</param>
        protected virtual void AddEmbedFields(EmbedBuilder builder) {
            foreach(int index in InputFields[PageNum].Keys) {
                // Add them as text
                MessageFieldNode mfn = InputFields[PageNum][index];

                if(index == CursorPos) {
                    builder.AddField($"> {mfn.Name}", mfn.GetValue());
                } else {
                    builder.AddField(mfn.Name, mfn.GetValue());
                }
            }
        }

        #region Utility
        /// <summary>
        /// Moves the cursor up num times.
        /// </summary>
        /// -C
        /// <param name="num">The number of times to move the cursor up. Default is 1.</param>
        protected void MoveCursorUp(int num = 1) {
            if (num == 0) return; // if num is 0 just do nothing

            if(num < 0) MoveCursorDown(-num); // if num is negative, use the other method with positive num

            // Move the cursor
            for (int i = 0; i < num; i++) {
                CursorPos++;

                if (CursorPos > FieldCount) {
                    // If the cursor goes out of bounds, correct it
                    CursorPos = 1;
                }
            }
        }

        /// <summary>
        /// Moves the cursor down num times.
        /// </summary>
        /// -C
        /// <param name="num">The number of times to move the cursor down. Default is 1.</param>
        protected void MoveCursorDown(int num = 1) {
            if (num == 0) return; // if num is 0 just do nothing

            if (num < 0) MoveCursorUp(-num); // if num is negative, use the other method with positive num

            // Move the cursor
            for (int i = 0; i < num; i++) {
                CursorPos--;

                if(CursorPos < 1) {
                    // If the cursor goes out of bounds, correct it
                    CursorPos = FieldCount;
                }
            }
        }

        /// <summary>
        /// Goes to the next page of inputs.
        /// </summary>
        /// -C
        protected void NextPage() {
            if(PageNum > PageCount) {
                return; // Don't let it get out of bounds!
            }

            PageNum++;  // Move the page
        }

        /// <summary>
        /// Goes to the previous page of inputs.
        /// </summary>
        /// -C
        protected void PrevPage() {
            if (PageNum < 1) {
                return; // Don't let it get out of bounds!
            }

            PageNum++;  // Move the page
        }
        #endregion

        /// <summary>
        /// Displays the embed.
        /// </summary>
        /// -C
        /// <param name="channel">The channel to display the embed in</param>
        /// <returns></returns>
        public override async Task Display(IMessageChannel channel = null) {
            await base.Display(channel);

            EventQueueManager.AddMessageEvent(this);
        }
    }
}
