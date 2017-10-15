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
            var rand = random;
            var bytes = new byte[3];
            rand.NextBytes(bytes);
            return new Color(bytes[0], bytes[1], bytes[2]);
        }
    }
}
