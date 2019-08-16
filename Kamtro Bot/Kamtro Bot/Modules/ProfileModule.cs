﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    public class ProfileModule : ModuleBase<SocketCommandContext>
    {
        [Command("rep")]
        [Alias("giverep")]
        public async Task RepUserAsync([Remainder] string username = "") {
            TimeSpan ts = new TimeSpan(7, 0, 0, 0);
            ts -= DateTime.Now - DateTime.Now.LastSunday();

            if (!UserDataManager.CanAddRep(BotUtils.GetGUser(Context))) {
                await ReplyAsync(BotUtils.KamtroText($"You have no more reputation points left to give! Resets in {ts.Days} day{((ts.Days == 1) ? "":"s")}, {ts.Hours} hour{((ts.Hours == 1) ? "" : "s")}, {ts.Minutes} minute{((ts.Minutes == 1) ? "" : "s")}, and {ts.Seconds} second{((ts.Seconds == 1) ? "" : "s")}"));
                return;
            }

            if (username == "") {
                int rep = UserDataManager.GetUserData(BotUtils.GetGUser(Context)).ReputationToGive;
                await ReplyAsync(BotUtils.KamtroText($"You have {rep} reputation point{(rep == 1 ? "":"s")} left to give. Resets in {ts.Days} day{((ts.Days == 1) ? "" : "s")}, {ts.Hours} hour{((ts.Hours == 1) ? "" : "s")}, {ts.Minutes} minute{((ts.Minutes == 1) ? "" : "s")}, and {ts.Seconds} second{((ts.Seconds == 1) ? "" : "s")}"));
                return;
            }

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name. Make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await AddRep(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, AddRep, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("profile")]
        [Alias("userprofile", "p")]
        public async Task ProfileAsync([Remainder] string username = "") {
            if(username == "") {
                // user's profile
                SocketGuildUser usr = Context.Guild.GetUser(Context.User.Id);

                ProfileEmbed pe = new ProfileEmbed(UserDataManager.GetUserData(usr), usr);
                await pe.Display(Context.Channel);
                return;
            }

            // else, the requested user's profile

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await Profile(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, Profile, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("setcolor")]
        [Alias("setprofilecolour", "spc", "setprofilecolor", "setcolour")]
        public async Task SetProfileColorAsync([Remainder] string col = "") {
            if(col.Length < 1) {
                // Add some random flavor - Arcy
                Random rnd = new Random();
                int r, g, b, h;
                r = rnd.Next(0, 256);
                g = rnd.Next(0, 256);
                b = rnd.Next(0, 256);
                h = rnd.Next(0, 16777216);

                await ReplyAsync(BotUtils.KamtroText($"You need to specify a color! Try !setprofilecolor #{h.ToString("X")} or !setprofilecolor {r} {g} {b}"));
                return;
            }

            List<string> args = col.Split(' ').ToList();

            if(args.Count == 1) {
                // it's a hex code
                if(col.Length < 6 || col.Length > 8) {
                    await ReplyAsync(BotUtils.KamtroText("You need to specify a valid hex code, or RGB values. For hex code, You can use ABCDEF, xABCDEF, #ABCDEF, or 0xABCDEF"));
                    return;
                }

                string hex;

                if(col.Length == 7) {
                    hex = col.Substring(1);
                } else if(col.Length == 8) {
                    hex = col.Substring(2);
                } else {
                    hex = col;
                }

                uint color;

                bool succ = uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture,  out color);

                if(!succ) {
                    await ReplyAsync(BotUtils.KamtroText("You need to specify a valid hex code. Hex codes can only contain the digits 0-9, as well as letters A-F in any case."));
                    return;
                }

                Color c = new Color(color);

                UserDataManager.GetUserData(BotUtils.GetGUser(Context)).ProfileColor = c.RawValue;
                UserDataManager.SaveUserData();

                // The parameter in the ToString call is for formatting. The x means hexidecimal format.
                await ReplyAsync(embed: new BasicEmbed("Set Profile Color", $"({c.R}, {c.G}, {c.B})\n#{c.RawValue.ToString("x")}", "Your Profile Color has been set to", UserDataManager.GetUserData(BotUtils.GetGUser(Context)).GetColor()).GetEmbed());
            } else if(args.Count() == 3) {
                byte r, g, b;

                bool succ = byte.TryParse(args[0], out r);
                succ &= byte.TryParse(args[1], out g);
                succ &= byte.TryParse(args[2], out b);

                if(!succ) {
                    await ReplyAsync(BotUtils.KamtroText("RGB values must not be less than 0, or larger than 255. They must also be valid numbers, made up of only digits 0-9, and must be seperated by ONLY a space (No commas!)"));
                    return;
                }

                Color c = new Color(r, g, b);
                UserDataManager.GetUserData(BotUtils.GetGUser(Context)).ProfileColor = c.RawValue;
                UserDataManager.SaveUserData();
                await ReplyAsync(embed: new BasicEmbed("Set Profile Color", $"({c.R}, {c.G}, {c.B})\n#{c.RawValue.ToString("x")}", "Your Profile Color has been set to", UserDataManager.GetUserData(BotUtils.GetGUser(Context)).GetColor()).GetEmbed());
            } else {
                // Add some random flavor - Arcy
                Random rnd = new Random();
                int r, g, b;
                r = rnd.Next(0, 256);
                g = rnd.Next(0, 256);
                b = rnd.Next(0, 256);

                await ReplyAsync(BotUtils.KamtroText($"You need to specify all three RGB values, and they must be seperated by a space only.\nEx: !setprofilecolor {r} {g} {b}"));
            }
        }

        [Command("setquote")]
        [Alias("setprofilequote", "sq")]
        public async Task SetProfileQuoteAsync([Remainder] string quote = "") {
            // Quotes cannot be too long
            if (quote.Length > 100)
            {
                await ReplyAsync(BotUtils.KamtroText("Quotes cannot exceed 100 characters in length."));
                return;
            }

            UserDataManager.GetUserData(BotUtils.GetGUser(Context)).Quote = quote;
            UserDataManager.SaveUserData();

            if(string.IsNullOrWhiteSpace(quote)) {
                await ReplyAsync(BotUtils.KamtroText("Quote has been removed."));
            } else {
                await ReplyAsync(BotUtils.KamtroText($"Quote has been set to \"{quote}\"."));
            }
        }

        #region Helper Methods
        private async Task AddRep(SocketUser user) {
            if(user.Id == Context.User.Id) {
                await ReplyAsync(BotUtils.KamtroText("You can't give a repuation point to yourself!"));
                return;
            }

            UserDataManager.AddRep(BotUtils.GetGUser(Context), Context.Guild.GetUser(user.Id));

            await ReplyAsync(BotUtils.KamtroText($"{Context.User.Username} has given a reputation point to {user.Username}"));
        }

        private async Task Profile(SocketGuildUser user) {
            ProfileEmbed pe = new ProfileEmbed(UserDataManager.GetUserData(user), user);
            await pe.Display(Context.Channel);
        }
        #endregion
    }
}
