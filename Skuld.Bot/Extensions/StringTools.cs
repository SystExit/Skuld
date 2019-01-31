using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Skuld.Bot.Extensions
{
    public static class StringTools
    {
        private static readonly Dictionary<string, int> alphabet = new Dictionary<string, int>
        {
            { "a", 0 },
            { "b", 1 },
            { "c", 2 },
            { "d", 3 },
            { "e", 4 },
            { "f", 5 },
            { "g", 6 },
            { "h", 7 },
            { "i", 8 },
            { "j", 9 },
            { "k", 10 },
            { "l", 11 },
            { "m", 12 },
            { "n", 13 },
            { "o", 14 },
            { "p", 15 },
            { "q", 16 },
            { "r", 17 },
            { "s", 18 },
            { "t", 19 },
            { "u", 20 },
            { "v", 21 },
            { "w", 22 },
            { "x", 23 },
            { "y", 24 },
            { "z", 25 }
        };

        private static readonly string[] alphaDance =
        {
            "<a:adance:531506829470203914>",
            "<a:bdance:531506829965000745>",
            "<a:cdance:531506829541376000>",
            "<a:ddance:531506829516210180>",
            "<a:edance:531506829646364673>",
            "<a:fdance:531506829667205130>",
            "<a:gdance:531506831189868544>",
            "<a:hdance:531506831248326663>",
            "<a:idance:531506831869214740>",
            "<a:jdance:531506831403778058>",
            "<a:kdance:531506831932260362>",
            "<a:ldance:531506831441526795>",
            "<a:mdance:531506835962986534>",
            "<a:ndance:531506832254959636>",
            "<a:odance:531506833677090817>",
            "<a:pdance:531506833400266754>",
            "<a:qdance:531506833265786880>",
            "<a:rdance:534392649025585153>",
            "<a:sdance:531506832859201550>",
            "<a:tdance:531506833328701451>",
            "<a:udance:531506833328701440>",
            "<a:vdance:531506833911971864>",
            "<a:wdance:531506833874092072>",
            "<a:xdance:531506833362255883>",
            "<a:ydance:531506833471438848>",
            "<a:zdance:531506835123863574>"
        };

        private static readonly Regex symbols = new Regex("^[a-zA-Z0-9 ]*$");

        public static string ToRegionalIndicator(this string value)
        {
            string ret = "";

            foreach (var character in value)
            {
                if (!symbols.IsMatch(Convert.ToString(character)))
                    ret += character;
                else if (!Char.IsWhiteSpace(character))
                    ret += ":regional_indicator_" + character + ": ";
                else
                    ret += " ";
            }

            return ret;
        }

        public static string ToDancingEmoji(this string value)
        {
            string ret = "";

            foreach(var chr in value)
            {
                if(!symbols.IsMatch(Convert.ToString(chr)))
                {
                    ret += chr;
                    continue;
                }
                if(!char.IsWhiteSpace(chr))
                {
                    ret += alphaDance[alphabet.FirstOrDefault(x => x.Key == chr.ToString().ToLower()).Value];
                }
                else
                {
                    ret += ' ';
                }
            }

            return ret;
        }
    }
}
