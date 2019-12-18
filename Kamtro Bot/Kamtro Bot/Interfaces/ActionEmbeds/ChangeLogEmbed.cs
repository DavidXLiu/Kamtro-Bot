using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class ChangeLogEmbed : ActionEmbed
    {
        public static List<string> LogOrder = LoadLogOrder();

        private static readonly MenuOptionNode LAST = new MenuOptionNode(ReactionHandler.DOWN_STR, "Last Version");
        private static readonly MenuOptionNode NEXT = new MenuOptionNode(ReactionHandler.UP_STR, "Next Version");

        private string Version;
        private int VPos = LogOrder.Count - 1;

        /// <summary>
        /// Change log embed
        /// </summary>
        /// <remarks>
        /// The versions to jump to are normalized, meaning any zeroes to the right of a number are trimmed if that's all the rest of the version is
        /// This means that 1.0 becomes 1, 0.9.0 becomes 0.9, and 1.0.9 stays the same.
        /// </remarks>
        /// <param name="ctx">Command Context</param>
        /// <param name="version">The version to jump to (Latest version if unspecified)</param>
        public ChangeLogEmbed(SocketCommandContext ctx, string version = "DEFAULT") {
            SetCtx(ctx);

            if(version != "DEFAULT") {
                for(int i = LogOrder.Count - 1; i >= 0; i--) {
                    string s = LogOrder[i];
                    if (NormalizeVersion(s) == NormalizeVersion(version)) {
                        VPos = i;
                        break;
                    }
                }
            }

            Version = LogOrder[VPos];

            AddMenuOptions(LAST, NEXT);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Changelog");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());

            string entry;

            if(!VersionFileExists()) {
                entry = "No File Found. Please ping Arcy or Carbon";
            } else {
                if(LogOrder.Count == 0) {
                    eb.AddField("Version ???", "LOG FILE IS EMPTY.\nPlease ping arcy or carbon.");
                    return eb.Build();
                } 

                entry = string.IsNullOrWhiteSpace(GetLogEntry()) ? "[Empty]" : GetLogEntry();
            }

            eb.AddField(Version, entry);

            if(Program.Debug) {
                eb.AddField("Debug", GetLogPath());
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction button) {
            if (LogOrder.Count == 0) return;  // Don't bother with actions if log file is empty
            
            switch (button.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    // go to next version
                    NextVersion();

                    if(VPos >= LogOrder.Count - 1) {
                        // if it's at the latest version, take out the next button in the footer
                        ClearMenuOptions();
                        AddMenuOptions(LAST);
                    } else {
                        ClearMenuOptions();
                        AddMenuOptions(NEXT, LAST);
                    }

                    await UpdateEmbed();
                    break;

                case ReactionHandler.DOWN_STR:
                    // go to last version
                    PrevVersion();

                    if (VPos <= 0) {
                        // if it's at the first version, take out the next button in the footer
                        ClearMenuOptions();
                        AddMenuOptions(NEXT);
                    } else {
                        ClearMenuOptions();
                        AddMenuOptions(NEXT, LAST);
                    }

                    await UpdateEmbed();
                    break;
            }
        }

        private bool VersionFileExists() {
            return File.Exists(GetLogPath());
        }

        private string GetLogEntry() {
            return FileManager.ReadFullFile(GetLogPath());
        }

        private string GetLogPath() {
            return $@"Change Log\log_{Version}.txt";
        }

        private void NextVersion() {
            if(VPos < LogOrder.Count - 1) {
                VPos++;
            }

            Version = LogOrder[VPos];
        }

        private void PrevVersion() {
            if (VPos > 0) {
                VPos--;
            }

            Version = LogOrder[VPos];
        }

        private string NormalizeVersion(string v) {
            List<string> vs = v.Split('.').ToList();

            for(int i = vs.Count-1; i >= 0; i--) {
                if (vs[i] == "0") vs.RemoveAt(i);
                else break;
            }

            string s = "";

            foreach(string vv in vs) {
                s += vv + ".";
            }

            return s.Trim('.');
        }

        private static List<string> LoadLogOrder() {
            string order = FileManager.ReadFullFile(DataFileNames.ChangeLogOrderFile);
            List<string> ret = new List<string>();

            foreach(string v in order.Split('\n')) {
                ret.Add(v.Trim());
            }

            return ret;
        }
    }
}
