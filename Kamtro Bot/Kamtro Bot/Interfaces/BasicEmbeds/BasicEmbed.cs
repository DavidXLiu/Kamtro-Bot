using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// Basic embed. Only text, no menu options.
    /// </summary>
    /// -C
    public class BasicEmbed : KamtroEmbedBase
    {
        public string Title;
        public string Text;
        public string FieldName;
        public Color Col;
        public string Url;

        public BasicEmbed(string title, string text, string fieldName, Color col, string url = "") {
            Title = title;
            Text = text;
            FieldName = fieldName;
            Col = col;
            Url = url;
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(Title);
            builder.WithColor(Col);
            builder.AddField(FieldName, Text);

            if (!string.IsNullOrWhiteSpace(Url)) builder.WithThumbnailUrl(Url);

            return builder.Build();
        }
    }
}
