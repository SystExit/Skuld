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
    }
}