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
        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Skuld = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetEntryAssembly().GetName(), new GitRepoStruct("Skuldbot", "Skuld"));
        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> SysEx = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(SysExClient)).GetName(), new GitRepoStruct("SystemExit", "SysEx.Net"));
        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Booru = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(BooruClient)).GetName(), new GitRepoStruct("SystemExit", "Booru.Net"));
        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Weebsh = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(WeebClient)).GetName(), new GitRepoStruct("Daniele122898", "Weeb.net"));
        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Twitch = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(TwitchHelixClient)).GetName(), new GitRepoStruct("Akitaux", "Twitch"));
    }

    public struct GitRepoStruct
    {
        public string Owner { get; set; }
        public string Repo { get; set; }

        public GitRepoStruct(string o, string r)
        {
            Owner = o;
            Repo = r;
        }
    }
}