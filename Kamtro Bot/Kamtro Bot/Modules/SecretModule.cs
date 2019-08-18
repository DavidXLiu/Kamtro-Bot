using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Modules
{
    public class SecretModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Dergbutt secret command
        /// Sends a certain image. Art Credit: Shade
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("dergbutt")]
        public async Task DergbuttAsync([Remainder] string message = "")
        {
            await Context.Channel.SendFileAsync("Images/Dergbutt.png");
        }

        /// <summary>
        /// Mlem secret command
        /// Sends a certain image. Art Credit: Jenni the dragon
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("mlem")]
        public async Task MlemAsync([Remainder] string message = "")
        {
            await Context.Channel.SendFileAsync("Images/Mlem.png");
        }

        /// <summary>
        /// Olive Garden secret command
        /// A copy pasta made by Retrospecter
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("olivegarden")]
        public async Task OliveGardenAsync([Remainder] string message = "")
        {
            string copyPasta = "Ever wonder what I would do to your right pinky toe?" +
                " I'd take it out to a night at Olive Garden, but only get the breadsticks since that'd be a little expensive if we got more than just that. " +
                "As you take a bite out of the breadstick, I'll slowly reach down towards your hamstring and give you a ham steak to sautee that big boy. " +
                "Afterwards we'll head home and I'll slowly lie you down onto the cold tile floor and slide you across it like a cat on a towel. " +
                "It's pretty fun. Anyways, once I slide you into the room like a cute kitty I'll slither on top of you and pull a bread stick out of my chest tuft. " +
                "Remember this breadstick? It's the one we got from Olive Garden. I slowly put it in your mouth and tell you to eat it, because that's all we got til Tuesday bitch.﻿";

            await ReplyAsync(BotUtils.KamtroText(copyPasta));
        }

        /// <summary>
        /// Porter secret command
        /// Sends certain images. This is exclusive to people that donated to Porter's fundraiser
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("porter")]
        public async Task PorterAsync([Remainder] string message = "")
        {
            if (UserDataManager.GetUserData(BotUtils.GetGUser(Context)).PorterSupporter)
            {
                Random random = new Random();
                string porterEmoji = CustomEmotes.porterEmojis[random.Next(0, CustomEmotes.porterEmojis.Length)];

                await Context.Channel.SendMessageAsync(porterEmoji);
            }
        }

        /// <summary>
        /// Snokbutt secret command
        /// Sends certain images. Art Credit: Retrospecter, Gaily
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("snokbutt")]
        public async Task SnokbuttAsync([Remainder] string message = "")
        {
            Random random = new Random();
            int num = random.Next(0, 100);
            if (num == 0) await Context.Channel.SendFileAsync("Images/Snokbutt2.png");
            else await Context.Channel.SendFileAsync("Images/Snokbutt.png");
        }

        /// <summary>
        /// Vore secret command
        /// 20% chance to send the Arcy emote.
        /// Author: Arcy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("vore")]
        public async Task VoreAsync([Remainder] string message = "")
        {
            Random random = new Random();
            int num = random.Next(0, 5);
            if (num == 0) await ReplyAsync(CustomEmotes.Arcy);
        }
    }
}
