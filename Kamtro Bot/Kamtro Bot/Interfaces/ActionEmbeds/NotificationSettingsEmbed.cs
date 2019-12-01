using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class NotificationSettingsEmbed : ActionEmbed {
        public static readonly List<string> VarNames = new List<string>(new string[] { "UpdateNotify", "ItemNotify", "RepNotify", "TitleNotify" });
        public static readonly List<string> DisplayNames = new List<string>(new string[] { "Bot Updates", "Items", "Reputation Points", "Titles" });
        public static readonly List<string> Descriptions = new List<string>(new string[] {
            "When a new update for Kamtro bot is announced, Kamtro will DM you the information about the update.",
            "Upon earning an item, Kamtro will DM you the item you have obtained.",
            "Upon earning a reputation point, Kamtro will DM you the user that gave you a reputation point.",
            "Upon earning a title, Kamtro will DM you or announce in the bot channel depending on the difficulty of the title. Turning this setting off will stop Kamtro from DMing you when you earn a title."
            
        });

        private int Cursor = 0;
        private UserSettingsNode Settings;

        public NotificationSettingsEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            Settings = UserDataManager.GetUserSettings(BotUtils.GetGUser(ctx));

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.DONE);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Notification Settings");
            eb.WithColor(BotUtils.Kamtro);

            bool on;

            string settingList = "";
            string desc = "";
            string cursor;

            foreach (FieldInfo f in Settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                if(f.FieldType == typeof(bool) && VarNames.Contains(f.Name)) {
                    int pos = VarNames.IndexOf(f.Name);

                    if(pos >= 0 && pos < VarNames.Count) {
                        on = (f.GetValue(Settings) as bool?).Value;

                        if(pos == Cursor) {
                            cursor = CustomEmotes.CursorAnimated;
                        } else {
                            cursor = CustomEmotes.CursorBlankSpace;
                        }

                        if(on) {
                            settingList += $"{cursor}**{DisplayNames[pos]}**\n";
                        } else {
                            settingList += $"{cursor}{DisplayNames[pos]}\n";
                        }
                    }
                }
            }

            eb.AddField("Choose a setting", string.IsNullOrWhiteSpace(settingList) ? BotUtils.ZeroSpace : settingList);

            eb.WithDescription(string.IsNullOrWhiteSpace(desc) ? BotUtils.ZeroSpace : desc);

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch (option.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    await CursorDown(); // I know these are swapped it's all wacky
                    break;

                case ReactionHandler.DOWN_STR:
                    await CursorUp();
                    break;

                case ReactionHandler.SELECT_STR:
                    // Toggle Thingy
                    if (Cursor >= VarNames.Count || Cursor < 0 || VarNames.Count == 0) return;

                    //Settings.GetType().GetProperty(VarNames[Cursor]).SetValue(Settings, !(Settings.GetType().GetProperty(VarNames[Cursor]).GetValue(null) as bool?).Value, null);

                    FieldInfo f = Settings.GetType().GetField(VarNames[Cursor]);
                    f.SetValue(Settings, !(f.GetValue(Settings) as bool?).Value);

                    UserDataManager.SaveUserSettings();
                    await UpdateEmbed();
                    break;

                default:
                    break;
            }
        }

        private async Task CursorUp() {
            Cursor++;

            if (Cursor >= VarNames.Count) Cursor = 0;

            await UpdateEmbed();
        }

        private async Task CursorDown() {
            Cursor--;
            if (Cursor < 0) Cursor = VarNames.Count - 1;

            await UpdateEmbed();
        }
    }
}
