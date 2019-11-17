using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// Role selection Embed
    /// </summary>
    /// <remarks>
    /// This wacky embed was made before the UpdateEmbed() method existed, so it relied on shenanigans to get it's stuff to work.
    /// </remarks>
    public class RoleEmbed : ActionEmbed
    {
        private int cursorPos = 0;  // How many spaces down the cursor is

        private bool boldOverride = false;
        private bool boldNoverride = false;
        private ulong boldId = 0;

        public RoleEmbed(SocketCommandContext ctx, SocketGuildUser user) {
            // This method call adds all of the menu options to the array (Located in the base class)
            // Each option is added as a new MenuOptionNode object.
            // The last node passed in this specific call is one that's located in the ReactionHandler class
            // It's for the Done button.
            AddMenuOptions(ReactionHandler.UP, ReactionHandler.DOWN,
                ReactionHandler.SELECT, ReactionHandler.DONE);

            SetCtx(ctx);
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Kamtro Roles");

            builder.WithAuthor("Role List");
            builder.WithDescription($"Hover over a role to see it's description\nRoles in **Bold** are ones you already have");

            string roleList = "";
            string cursor;  // The type of cursor
            bool shouldBeBold;  // If the line should be bold, as in if the user already has the role
            bool onId;



            for(int i = 0; i < ServerData.ModifiableRoles.Count; i++) {
                if(cursorPos == i) {
                    // If the cursor is on this line
                    cursor = CustomEmotes.CursorAnimated; // Show it
                    //cursor = ">";
                } else {
                    // otherwise
                    cursor = CustomEmotes.CursorBlankSpace;  // Don't 
                    //cursor = " ";
                }

                onId = ServerData.ModifiableRoles[i].Id == boldId;
                shouldBeBold = BotUtils.GetGUser(Context).HasRole(ServerData.ModifiableRoles[i]);   // if the user has the role, make it bold.
                shouldBeBold = shouldBeBold || (boldOverride && onId);
                shouldBeBold = shouldBeBold && (!boldNoverride || !onId);

                if (boldOverride && ServerData.ModifiableRoles[i].Id == boldId) {
                    boldOverride = false; 
                    boldId = 0;
                }

                if(boldNoverride && ServerData.ModifiableRoles[i].Id == boldId) {
                    boldNoverride = false;
                    boldId = 0;
                }

                roleList += ((i == 0) ? "" : "\n") + cursor + MakeBold(ServerData.ModifiableRoles[i].Name, shouldBeBold);
            }

            uint colorHex = Convert.ToUInt32(Program.Settings.RoleDescriptions[ServerData.ModifiableRoles[cursorPos].Id].Color.Replace("#", ""), 16);
            Color embedColor = new Color(colorHex);
            builder.WithColor(embedColor);

            builder.AddField("Roles", roleList);

            // Get the description from the settings class
            string roleDesc = Program.Settings.RoleDescriptions[ServerData.ModifiableRoles[cursorPos].Id].Description;
            builder.AddField("Description", roleDesc);

            if(Program.Debug) {
                builder.AddField("Debug Info", $"MRFC: {ServerData.ModifiableRoles.Count} C: {cursorPos}\nBO: {boldOverride} BN: {boldNoverride} ");
            }

            AddMenu(builder);  // Adds the menu key at the bottom

            return builder.Build();  // Build the embed and return it
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    cursorPos--;  // Move the cursor up a space
                    break;

                case ReactionHandler.DOWN_STR:
                    cursorPos++;  // Move the cursor down a space
                    break;

                case ReactionHandler.SELECT_STR:
                    if (!BotUtils.GetGUser(Context).HasRole(ServerData.ModifiableRoles[cursorPos])) {
                        // If the user doesn't have the role
                        await BotUtils.GetGUser(Context).AddRoleAsync(ServerData.ModifiableRoles[cursorPos]);  // Give the user the role
                        boldOverride = true;
                    } else {
                        // If the user does have the role
                        await BotUtils.GetGUser(Context).RemoveRoleAsync(ServerData.ModifiableRoles[cursorPos]);  // Remove it
                        boldNoverride = true;
                    }
                    boldId = ServerData.ModifiableRoles[cursorPos].Id;
                    break;

                default:
                    break;
            }

            // Wrap the cursor if it goes past the top
            if(cursorPos < 0) {
                cursorPos = ServerData.ModifiableRoles.Count - 1;
            } else if(cursorPos >= ServerData.ModifiableRoles.Count) {
                cursorPos = 0;
            }

            await Message.ModifyAsync(msg => msg.Embed = GetEmbed());  // Edit the message
        }

        /// <summary>
        /// Makes the input either bold, or does not change it.
        /// </summary>
        /// <param name="inp">The text to be bolded</param>
        /// <param name="bold">Whether it should be bold.</param>
        /// <returns>The bolded or not bolded text.</returns>
        private string MakeBold(string inp, bool bold) {
            if(bold) {
                return $"**{inp}**";
            }

            return inp;
        }
    }
}
