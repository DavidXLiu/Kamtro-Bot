using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class HelpEmbed : ActionEmbed
    {
        public const string GoBack = "⏫";

        private int Cursor;
        private int FileCount;
        private string Path;
        private bool Admin;

        public HelpEmbed(SocketCommandContext ctx, bool admin = false) {
            AddMenuOptions(ReactionHandler.SELECT, new MenuOptionNode(GoBack, "Go back"), ReactionHandler.UP, ReactionHandler.DOWN);
            SetCtx(ctx);
            Path = @"Help";
            Cursor = 0;
            Admin = admin;
            FileCount = HelpManager.GetDirList(Path, Admin).Length;
        }

        public async override Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    await CursorDown(); // these are swapped because I messed something up. They should remain swapped (CursorDown under the UP_STR case and vice versa)
                    break;

                case ReactionHandler.DOWN_STR:
                    await CursorUp();
                    break;

                case ReactionHandler.SELECT_STR:
                    if (HelpManager.GetDirList(Path, Admin).Count() < 1) return;
                    await Select(HelpManager.GetDirList(Path, Admin)[Cursor]);
                    break;

                case GoBack:
                    await Back();
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithColor(BotUtils.White);

            if (OnFile()) {
                eb.WithTitle("Command Info"); // If it's on a file, it's command info
                AddHelpPage(eb);
            } else {
                //eb.WithTitle("Help"); // If the page is on the directory select, it's the help menu
                AddDirList(eb);
            }

            AddMenu(eb);

            return eb.Build();
        }

        #region Helper Methods


        /// <summary>
        /// Gets the text for the help embed based off of the list of directories passed in, and the cursor position
        /// </summary>
        /// <param name="dirs">The list of Directories</param>
        /// <param name="pos">The cursor position</param>
        /// <returns>The text for the embed</returns>
        private string GetDirText(string[] dirs, int pos) {
            if (dirs.Length == 0) return "Nothing here.";

            string dl = "";

            for (int i = 0; i < dirs.Length; i++) {
                if (!dirs[i].Contains('.')) {
                    if (i == pos) dl += "> ";

                    // if it's a folder, make it bold
                    dl += $"**{HelpManager.StripExtraDirs(dirs[i])}**\n";
                } else {
                    if (i == pos) dl += "> ";

                    // else it's a file (no extension)
                    dl += HelpManager.StripExtraDirs(dirs[i].Substring(0, dirs[i].LastIndexOf('.'))) + "\n"; //  add it and trim off the end
                }
            }

            return dl.TrimEnd('\n');
        }

        /// <summary>
        /// Takes in the embed builder and adds the formatted help page.
        /// </summary>
        /// <param name="eb">The embed builder</param>
        private void AddHelpPage(EmbedBuilder eb) {
            if(BotUtils.GetFileExtension(Path) == "json") {
                // Interpret JSON
                HelpPageNode page = HelpManager.GetNode(Path);

                eb.AddField("Command Name", $"!{page.Name}");

                if(page.Alias.Length > 0) {
                    string alias = "!" + page.Alias[0];

                    for(int i = 1; i < page.Alias.Length; i++) {
                        alias += $", !{page.Alias[i]}";
                    }

                    eb.AddField("Aliases", alias);
                }

                eb.AddField("Usage", page.Usage);
                eb.AddField("Description", page.Description);

                if (!string.IsNullOrWhiteSpace(page.GifURL)) {
                    eb.AddField("Example:", BotUtils.ZeroSpace);
                    eb.WithImageUrl(page.GifURL);
                }
            } else {
                // Assume it's text. All other files are ignored anyways
                string text = FileManager.ReadFullFile(Path);
                eb.AddField(BotUtils.ZeroSpace, text);
            }
        }

        /// <summary>
        /// Adds the list of files and folders to the embed
        /// </summary>
        /// <param name="eb">The embed builder</param>
        private void AddDirList(EmbedBuilder eb) {
            eb.AddField(VirtualPath(), GetDirText(HelpManager.GetDirList(Path, Admin), Cursor));
        }

        private string VirtualPath() {
            return string.IsNullOrWhiteSpace(Path.Substring(4)) ? "Main Menu" : Path.Substring(4).TrimStart('\\').Replace('\\', '/');
        }

        private bool OnFile() {
            return Path.Contains('.');
        }

        private async Task Select(string dir) {
            if (FileCount == 0) return;

            Cursor = 0;
            Path = HelpManager.SelectDir(Path, dir);
            if (!OnFile()) FileCount = HelpManager.GetDirList(Path, Admin).Length;
            else FileCount = 0;

            await UpdateEmbed();
        }

        private async Task Back() {
            Path = HelpManager.BackDir(Path);
            FileCount = HelpManager.GetDirList(Path, Admin).Length;

            await UpdateEmbed();
        }

        private async Task CursorUp() {
            if (FileCount == 0) return;
            Cursor++;

            if (Cursor >= FileCount) Cursor = 0;

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (FileCount == 0) return;
            Cursor--;
            if (Cursor < 0) Cursor = FileCount - 1;

            await UpdateEmbed();
        }
        #endregion
    }
}
