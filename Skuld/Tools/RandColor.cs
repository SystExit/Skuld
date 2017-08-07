using System;
using Discord;

namespace Skuld.Tools
{
    public class RandColor
    {
        public static Color RandomColor()
        {
            var rand = Bot.random;
            Byte[] b = new byte[3];
            rand.NextBytes(b);
            return new Color(b[0], b[1], b[2]);
        }
    }
}
