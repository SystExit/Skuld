using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Skuld.Tools
{
    public class Tools : Bot
    {
        private static readonly string[] _validExtensions = { "jpg", "bmp", "gif", "png" };
        public static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }
        public static Color RandomColor()
        {
            var rand = Bot.random;
            Byte[] b = new byte[3];
            rand.NextBytes(b);
            return new Color(b[0], b[1], b[2]);
        }
    }
}
