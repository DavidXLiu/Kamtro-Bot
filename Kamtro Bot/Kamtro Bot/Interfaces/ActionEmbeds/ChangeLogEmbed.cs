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
        private static readonly MenuOptionNode NEXT = new MenuOptionNode(ReactionHandler.UP_STR, "NNEXT Version");

        private string Version;
        private int VPos = LogOrder.Count - 1;

        public ChangeLogEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            Version = LogOrder[VPos];

            AddMenuOptions(LAST);
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
                entry = string.IsNullOrWhiteSpace(GetLogEntry()) ? BotUtils.ZeroSpace : GetLogEntry();
            }

            eb.AddField(Version, entry);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction button) {
            switch (button.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    // go to next version
                    NextVersion();

                    if(VPos >= LogOrder.Count - 1) {
                        // if it's at the latest version, take out the next button in the footer
                        ClearMenuOptions();
                        AddMenuOptions(LAST);
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

        private static List<string> LoadLogOrder() {
            string order = FileManager.ReadFullFile(DataFileNames.ChangeLogOrderFile);
            List<string> ret = new List<string>();

            foreach(string v in order.Split('\n')) {
                ret.Add(v);
            }

            return ret;
        }
    }
}
