using Booru.Net;
using SysEx.Net;
using System;
using System.Reflection;
using Weeb.net;

namespace Skuld.Core.Utilities.Stats
{
    public static class SoftwareStats
    {
        public static readonly OperatingSystem WindowsVersion = Environment.OSVersion;
        public static readonly AssemblyName Skuld = Assembly.GetEntryAssembly().GetName();
        public static readonly AssemblyName SysEx = Assembly.GetAssembly(typeof(SysExClient)).GetName();
        public static readonly AssemblyName Booru = Assembly.GetAssembly(typeof(BooruClient)).GetName();
        public static readonly AssemblyName Weebsh = Assembly.GetAssembly(typeof(WeebClient)).GetName();
    }
}