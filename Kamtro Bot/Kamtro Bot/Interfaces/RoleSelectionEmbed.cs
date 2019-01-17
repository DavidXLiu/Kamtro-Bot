using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces
{
    public class RoleSelectionEmbed : KamtroEmbedBase
    {
        private const string UP = "⬆️";
        private const string DOWN = "⬇️";
        private const string SELECT = "🔷";

        private static readonly int maxCursorPos = ServerData.ModifiableRoles.Count;  // The farthest the cursor should go
        private int cursorPos = 0;  // How many spaces down the cursor is
        private SocketGuildUser sender;  // The person who the embed is for


        public RoleSelectionEmbed(SocketGuildUser user) {
            // This method call adds all of the menu options to the array (Located in the base class)
            // Each option is added as a new MenuOptionNode object.
            // The last node passed in this specific call is one that's located in the ReactionHandler class
            // It's for the Done button.
            AddMenuOptions(new MenuOptionNode("⬆️", "Cursor up"),
                new MenuOptionNode("⬇️", "Cursor down"),
                new MenuOptionNode("🔷", "Select"),
                ReactionHandler.DONE_NODE);

            sender = user;  // Set the sender variable
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithAuthor("Role List");
            builder.WithDescription($"Hover over a role to see it's description\nRoles in **Bold** are ones you already have");

            string roleList = "";
            string cursor;  // The type of cursor
            bool shouldBeBold;  // If the line should be bold, as in if the user already has the role

            for(int i = 0; i < ServerData.ModifiableRoles.Count; i++) {
                if(cursorPos == i) {
                    // If the cursor is on this line
                    cursor = CustomEmotes.CursorAnimated; // Show it
                } else {
                    // otherwise
                    cursor = CustomEmotes.CursorBlankSpace;  // Don't 
                }


                shouldBeBold = sender.Roles.Contains(ServerData.ModifiableRoles[i]);  // if the user has the role, make it bold.
                roleList += MakeBold($"{((i == 0) ? "":"\n")}{cursor} {ServerData.ModifiableRoles[i].Name}", shouldBeBold);
            }

            builder.AddField("Roles", roleList);

            // Get the description from the settings class
            string roleDesc = Program.Settings.RoleDescriptions[ServerData.ModifiableRoles[cursorPos].Id].Description;
            builder.AddField("Description", roleDesc);

            AddMenu(builder);  // Adds the menu key at the bottom

            return builder.Build();  // Build the embed and return it
        }

        public override void PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case UP:
                    cursorPos++;
                    break;

                case DOWN:
                    cursorPos--;
                    break;

                case SELECT:
                    sender.AddRoleAsync(ServerData.ModifiableRoles[cursorPos]);  // Give the user the role
                    break;

                default:
                    break;
            }

            // Wrap the cursor if it goes past the top
            if(cursorPos < 0) {
                cursorPos = maxCursorPos;
            } else if(cursorPos >= maxCursorPos) {
                cursorPos = 0;
            }

            Message.ModifyAsync(msg => msg.Embed = GetEmbed());  // Edit the message
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inp"></param>
        /// <param name="bold"></param>
        /// <returns></returns>
        private string MakeBold(string inp, bool bold) {
            if(bold) {
                return $"**{inp}**";
            }

            return inp;
        }
    }
}
