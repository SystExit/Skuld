using Akitaux.Twitch.Helix;
using Booru.Net;
using SysEx.Net;
using System;
using System.Collections.Generic;
using System.Reflection;
using Weeb.net;

namespace Skuld.Core.Utilities.Stats
{
    public static class SoftwareStats
    {
        public static readonly OperatingSystem WindowsVersion = Environment.OSVersion;
        public static readonly KeyValuePair<AssemblyName, string> Skuld = new KeyValuePair<AssemblyName, string>(Assembly.GetEntryAssembly().GetName(), "https://github.com/exsersewo/Skuld");
        public static readonly KeyValuePair<AssemblyName, string> SysEx = new KeyValuePair<AssemblyName,string>(Assembly.GetAssembly(typeof(SysExClient)).GetName(), "https://github.com/exsersewo/SysEx.Net");
        public static readonly KeyValuePair<AssemblyName, string> Booru = new KeyValuePair<AssemblyName,string>(Assembly.GetAssembly(typeof(BooruClient)).GetName(), "https://github.com/exsersewo/Booru.Net");
        public static readonly KeyValuePair<AssemblyName, string> Weebsh = new KeyValuePair<AssemblyName, string>(Assembly.GetAssembly(typeof(WeebClient)).GetName(), "https://github.com/Daniele122898/Weeb.net");
        public static readonly KeyValuePair<AssemblyName, string> Twitch = new KeyValuePair<AssemblyName, string>(Assembly.GetAssembly(typeof(TwitchHelixClient)).GetName(), "https://github.com/Akitaux/Twitch");
    }
}