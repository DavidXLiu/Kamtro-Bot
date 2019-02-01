using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces
{
    public class UserSelectionEmbed : KamtroEmbedBase
    {
        // The unicodes of the emojis IN ORDER starting with  and ending in 🔟
        private static readonly string[] NUMBER_LIST = new string[] { "\U00000039\U0000fe0f\U000020e3", ""};


        private delegate Func<Task> selectedAction(SocketGuildUser user);

        private List<SocketGuildUser> UserOptions;
        private string EmbedMessage;
        
        public UserSelectionEmbed(List<SocketGuildUser> users, Func<SocketGuildUser, Task> action, string message = "There were multiple users with that name!") {
            UserOptions = users;
            EmbedMessage = message;

            selectedAction sa = new selectedAction(action);
            
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Select a User");
            builder.WithColor(Color.Blue);
            
            if(UserOptions.Count > 10) {
                builder.AddField("That name is way too vague!", "");
                return builder.Build();
            }

            return builder.Build();
        }

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
