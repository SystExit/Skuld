using System;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Skuld.Utilities
{
    public static class EmbedUtils
    {
        public static Color RandomColor()
        {
            var bytes = new byte[3];
            Bot.services.GetRequiredService<Random>().NextBytes(bytes);
            return new Color(bytes[0], bytes[1], bytes[2]);
        }
    }
}
