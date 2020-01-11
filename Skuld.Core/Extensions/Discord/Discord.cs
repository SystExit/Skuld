using Discord;

namespace Skuld.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static Color FromHex(this string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            return new Color(col.R, col.G, col.B);
        }

        public static string ToHex(this Color color)
            => System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(255, color.R, color.G, color.B));
    }
}