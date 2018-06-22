using System;
using System.Reflection;
using SysEx.Net;
using Booru.Net;
using Weeb.net;

namespace Skuld.Tools.Stats
{
    public class SoftwareStats
    {
        public readonly OperatingSystem WindowsVersion;
        public readonly AssemblyName Skuld;
        public readonly AssemblyName SysEx;
        public readonly AssemblyName Booru;
        public readonly AssemblyName Weebsh;

        public SoftwareStats()
        {
            Skuld = Assembly.GetEntryAssembly().GetName();
            SysEx = Assembly.GetAssembly(typeof(SysExClient)).GetName();
            Booru = Assembly.GetAssembly(typeof(BooruClient)).GetName();
            Weebsh = Assembly.GetAssembly(typeof(WeebClient)).GetName();
            WindowsVersion = Environment.OSVersion;
        }
    }
}
