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
using Kamtro_Bot.Handlers;
using Discord.Commands;

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
        public new TimeSpan Timeout = new TimeSpan(0, 1, 0);

        // Storage for the input fields
        // First key is the page, second key is the position on the page
        // -C
        public Dictionary<int, Dictionary<int, MessageFieldNode>> InputFields = new Dictionary<int, Dictionary<int, MessageFieldNode>>();

        public int PageCount = 0;
        public int PageNum = 1;
        public int CursorPos = 1;

        /// <summary>
        /// This is the method called when a message has been recieved by a user who has an active embed.
        /// </summary>
        /// <remarks>
        /// This seems to be a duplicate of <see cref="PerformMessageAction(SocketUserMessage)"/>, no references in code so
        /// this will not be used. It will be kept until needed, might be removed at a later time.
        /// </remarks>
        /// <param name="message">The message sent by the user</param>
        /// <returns></returns>
        public virtual async Task OnMessage(SocketUserMessage message) {
            // PerformMessageAction(message);
        }

        /// <summary>
        /// This is the method that will be called when the user sends a message in the bot channel if the interface is waiting on a message.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that the message is from the correct user in the correct channel when this is called.
        /// </remarks>
        /// -C
        /// <param name="message">The message that was sent by the user</param>
        public virtual async Task PerformMessageAction(SocketUserMessage message) {
            if (!InputFields.ContainsKey(PageNum) || !InputFields[PageNum].ContainsKey(CursorPos)) return;  // Safeguard

            MessageFieldNode mfn = InputFields[PageNum][CursorPos];
            string msg = message.Content;

            switch(mfn.Type) {
                case FieldDataType.BOOL:
                    bool b_input;

                    if (!bool.TryParse(msg, out b_input)) {
                        return;  // If it's invalid, return
                    }
                    mfn.SetValue(b_input.ToString());
                    break;

                case FieldDataType.INT:
                    int i_input;

                    if(!int.TryParse(msg, out i_input)) {
                        return;
                    }
                    mfn.SetValue(i_input.ToString());
                    break;

                case FieldDataType.ULONG:
                    ulong u_input;

                    if (!ulong.TryParse(msg, out u_input)) {
                        return;
                    }
                    mfn.SetValue(u_input.ToString());
                    break;

                case FieldDataType.DBL:
                    double d_input;

                    if (!double.TryParse(msg, out d_input)) {
                        return;  // If it's invalid, return
                    }
                    mfn.SetValue(d_input.ToString());
                    break;

                default:
                    mfn.SetValue(msg);
                    break;
            }

            await Message.ModifyAsync(_msg => _msg.Embed = GetEmbed()); // update the embed
        }

        /// <summary>
        /// Searches the class for all fields marked with <see cref="InputField"/>,
        /// and adds them to the internal dictionary for use in other methods.
        /// </summary>
        /// <remarks>
        /// Important to note:
        /// Be careful with how you arrange these fields.
        /// If a field is in a spot that is already occupied, 
        /// It will throw an error. 
        /// </remarks>
        /// -C
        protected void RegisterMenuFields() {
            bool arrows = false;

            foreach(FieldInfo f in GetType().GetFields()) {
                if(Attribute.IsDefined(f, typeof(InputField))) {
                    // if it's an input field var
                    InputField info = f.GetCustomAttribute(typeof(InputField)) as InputField; // get it's attribute

                    int page = info.Page;
                    int pos = info.Position;

                    if(!InputFields.ContainsKey(page)) {
                        InputFields[page] = new Dictionary<int, MessageFieldNode>();
                    }

                    if(InputFields[page].ContainsKey(pos)) {
                        // If there is already an input field here
                        throw new ConflictingFieldException(InputFields[page][pos].Name, info.Name, page, pos); // Throw an error.
                    }

                    // Otherwise, if everything is fine,
                    // Add the field.
                    MessageFieldNode node = new MessageFieldNode(info.Name, info.Page, info.Position, info.Value, info.Type); // create the node
                    node.ClassPtr = this; // give it a pointer to this class, so it can modify the variable it's attached to
                    InputFields[info.Page][info.Position] = node;  // And add the appropriate node to the dict where it belongs
                }
            }

            int FieldCount = 0;

            foreach(int i in InputFields.Keys) {
                foreach(int j in InputFields[i].Keys) {
                    FieldCount++;
                    if (FieldCount > 1) arrows = true;
                }

                FieldCount = 0;
            }

            if(arrows) {
                // if there is more than one field, add up and down buttons
                AddMenuOptions(ReactionHandler.UP, ReactionHandler.DOWN);
            }

            PageCount = InputFields.Values.Count; // set the number of pages
        }

        /// <summary>
        /// Adds the embed fields to the embed, with the cursor.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -C
        /// <param name="builder">The embed builder</param>
        protected virtual void AddEmbedFields(EmbedBuilder builder) {
            if (!InputFields.ContainsKey(PageNum)) return;

            foreach(int index in InputFields[PageNum].Keys) {
                // Add them as text
                MessageFieldNode mfn = InputFields[PageNum][index];

                if(index == CursorPos) {
                    builder.AddField($"{CustomEmotes.CursorAnimated} {mfn.Name}", mfn.GetValue());
                } else {
                    builder.AddField(CustomEmotes.CursorBlankSpace + mfn.Name, mfn.GetValue());
                }
            }
        }

        /// <summary>
        /// This method is called when a reaction is passed in. It is not to be overridden.
        /// </summary>
        /// <remarks>
        /// This method is implemented to prevent redundant code (such as having to check for up and down arrows each time).
        /// It calls the method ButtonAction after doing it's usual checks.
        /// 
        /// TODO:
        /// Add the left and right reactions for those controls.
        /// 
        /// -C
        /// </remarks>
        /// <param name="action">The reaction used.</param>
        /// <returns></returns>
        public async override Task PerformAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.DOWN_STR:
                    // These cursor movement methods will work here even if the interface has only one field. If a user
                    // manually adds one of these reactions, the cursor will move, then correct itself accordingly. -C
                    await MoveCursorDown(); 
                    break;

                case ReactionHandler.UP_STR:
                    await MoveCursorUp();
                    break;

                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveMessageEvent(this);
                    break;

                default:
                    await ButtonAction(action);  // if none of the predefined actions were used, it must be a custom action.
                    break;
            }
        }

        /// <summary>
        /// This method is called when a reaction is passed in.
        /// </summary>
        /// <remarks>
        /// This method will always need an implementation, as there will be a confirm button on every embed that needs this interface
        /// If there isn't, just leave the extension empty, it'll be fine.
        /// 
        /// -C
        /// </remarks>
        /// <param name="action">The reaction used.</param>
        /// <returns></returns>
        public abstract Task ButtonAction(SocketReaction action);

        #region Utility
        /// <summary>
        /// Moves the cursor up num times.
        /// </summary>
        /// -C
        /// <param name="num">The number of times to move the cursor up. Default is 1.</param>
        protected async Task MoveCursorDown(int num = 1) {
            if (num == 0) return; // if num is 0 just do nothing

            if (num < 0) await MoveCursorUp(-num); // if num is negative, use the other method with positive num

            // Move the cursor
            for (int i = 0; i < num; i++) {
                CursorPos--;

                if (CursorPos < 1) {
                    // If the cursor goes out of bounds, correct it
                    CursorPos = GetFieldCount();
                }
            }
        }

        /// <summary>
        /// Moves the cursor down num times.
        /// </summary>
        /// -C
        /// <param name="num">The number of times to move the cursor down. Default is 1.</param>
        protected async Task MoveCursorUp(int num = 1) {
            if (num == 0) return; // if num is 0 just do nothing

            if (num < 0) await MoveCursorDown(-num); // if num is negative, use the other method with positive num

            // Move the cursor
            for (int i = 0; i < num; i++) {
                CursorPos++;

                if (CursorPos > GetFieldCount()) {
                    // If the cursor goes out of bounds, correct it
                    CursorPos = 1;
                }
            }

            await Message.ModifyAsync(_msg => _msg.Embed = GetEmbed());
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

        /// <summary>
        /// Used to test if the cursor is on a variable for input
        /// </summary>
        /// <returns>True if the cursor is over an input variable, false otherwise.</returns>
        protected bool IsCursorOnVar() {
            if(InputFields[PageNum].ContainsKey(CursorPos)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests all fields to see if they are filled.
        /// </summary>
        /// <returns>True if all fields have been filled out, false otherwise.</returns>
        protected bool AllFieldsFilled() {
            for(int i = 1; i <= PageCount; i++) {
                foreach(int j in InputFields[i].Keys) {
                    if(string.IsNullOrEmpty(InputFields[i][j].GetValue())) {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the number of fields on the current page
        /// </summary>
        /// <returns>The number of fields on the current page</returns>
        protected int GetFieldCount() {
            if (!InputFields.ContainsKey(PageNum)) return 0;

            return InputFields[PageNum].Count();
        }
        
        protected override void SetCtx(SocketCommandContext ctx) {
            base.SetCtx(ctx);
            CommandChannel = ctx.Channel as SocketChannel;
        }
        #endregion

        /// <summary>
        /// Displays the embed.
        /// </summary>
        /// -C
        /// <param name="channel">The channel to display the embed in</param>
        /// <returns></returns>
        public override async Task Display(IMessageChannel channel = null, string message = "") {
            await base.Display(channel, message);

            EventQueueManager.AddMessageEvent(this);
        }
    }
}
