using System;
using Discord;
using Skuld.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Skuld.Utilities.Discord
{
    public static class EmbedUtils
    {
        public static Color RandomColor()
        {
            var bytes = new byte[3];
            HostService.Services.GetRequiredService<Random>().NextBytes(bytes);
            return new Color(bytes[0], bytes[1], bytes[2]);
        }
    }
}
