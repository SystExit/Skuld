using System;
using Discord;

namespace Skuld.Tools
{
    public class EmbedToText
    {
        public static string ConvertEmbedToText(Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!String.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!String.IsNullOrEmpty(embed.Description))
                message += embed.Description+"\n";
            foreach(var field in embed.Fields)
                message += $"__{field.Name}__\n{field.Value}\n\n";
            if (embed.Video.HasValue)
                message += embed.Video.Value.Url+"\n";
            if (embed.Thumbnail.HasValue)
                message += embed.Thumbnail.Value.Url + "\n";
            if (embed.Image.HasValue)
                message += embed.Image.Value.Url + "\n";
            if (embed.Footer.HasValue)
                message += $"`{embed.Footer.Value.Text}`";
            if (embed.Timestamp.HasValue)
                message += " | " + embed.Timestamp.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt");
            return message;
        }
    }
}
