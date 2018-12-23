using Discord;
using System;
using System.Diagnostics;

namespace Skuld.Core.Utilities
{
    public static class EmbedUtils
    {
        public static Color RandomColor(int seed = 0)
        {
            var bytes = new byte[3];

            if (seed == 0)
                seed = Process.GetCurrentProcess().StartTime.Millisecond;

            new Random(seed).NextBytes(bytes);

            return new Color(bytes[0], bytes[1], bytes[2]);
        }

        public static Embed EmbedImage(Uri imageurl, string embedtitle = "", string embeddesc = "")
            => new EmbedBuilder
                {
                    Title = embedtitle,
                    Description = embeddesc,
                    Color = RandomColor(),
                    ImageUrl = imageurl.OriginalString
                }.Build();
    }
}