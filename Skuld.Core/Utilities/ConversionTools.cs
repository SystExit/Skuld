using System;
using Discord;

namespace Skuld.Core.Utilities
{
    public static class ConversionTools
    {
        public static int ParseInt32OrDefault(string s)
        {
            if (Int32.TryParse(s, out int tmp))
                return tmp;
            else
                return 0;
        }
        public static uint ParseUInt32OrDefault(string s)
        {
            if (UInt32.TryParse(s, out uint tmp))
                return tmp;
            else
                return 0;
        }
        public static UInt64 ParseUInt64OrDefault(string input)
        {
            if (UInt64.TryParse(input, out ulong tmp))
                return tmp;
            else
                return 0;
        }

        public static Color HexToDiscordColor(string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            return new Color(col.R, col.G, col.B);
        }
    }
}
