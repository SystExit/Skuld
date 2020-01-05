using Akitaux.Twitch.Helix;
using Booru.Net;
using Discord;
using Skuld.Core.Utilities;
using SysEx.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Weeb.net;

namespace Skuld.Core
{
    public static class SkuldAppContext
    {
        public static string BaseDirectory => AppContext.BaseDirectory;
        public static string TargetFrameworkName => AppContext.TargetFrameworkName;
        public static string ConfigurationId { get; private set; }

        public const string Website = "https://skuldbot.uk";
        public const string WebsiteLeaderboard = Website+"/leaderboard";
        public const string LeaderboardExperience = WebsiteLeaderboard+"/experience";
        public const string LeaderboardMoney = WebsiteLeaderboard+"/money";

        public static void SetConfigurationId(string inId) => ConfigurationId = inId;

        public static LogSeverity GetLogLevel() => (LogSeverity)Enum.Parse(typeof(LogSeverity), GetEnvVar(Utils.LogLvlEnvVar));

        public static object GetData(string name) => AppContext.GetData(name);

        public static string GetEnvVar(string envvar, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process) => Environment.GetEnvironmentVariable(envvar, target);

        public static void SetSwitch(string switchName, bool isEnabled) => AppContext.SetSwitch(switchName, isEnabled);

        public static bool TryGetSwitch(string switchName, out bool isEnabled) => AppContext.TryGetSwitch(switchName, out isEnabled);

        public static string StorageDirectory = Path.Combine(BaseDirectory, "storage");

        public static string LogDirectory = Path.Combine(BaseDirectory, "logs");

        public static string TempDirectory = Path.Combine(StorageDirectory, "temp");

        public static string FontDirectory = Path.Combine(StorageDirectory, "fonts");

        public static string ProfileDirectory = Path.Combine(TempDirectory, "profile");

        public static string IMagickCache = Path.Combine(TempDirectory, "imagickCache");

        public static readonly OperatingSystem WindowsVersion = Environment.OSVersion;
        public static readonly MemoryStats Memory = new MemoryStats();

        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Skuld = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetEntryAssembly().GetName(), 
                                                                                                                               new GitRepoStruct("Skuldbot", "Skuld"));

        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> SysEx = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(SysExClient)).GetName(), 
                                                                                                                               new GitRepoStruct("exsersewo", "SysEx.Net"));

        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Booru = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(BooruClient)).GetName(), 
                                                                                                                               new GitRepoStruct("exsersewo", "Booru.Net"));

        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Weebsh = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(WeebClient)).GetName(), 
                                                                                                                               new GitRepoStruct("Daniele122898", "Weeb.net"));

        public static readonly KeyValuePair<AssemblyName, GitRepoStruct> Twitch = new KeyValuePair<AssemblyName, GitRepoStruct>(Assembly.GetAssembly(typeof(TwitchHelixClient)).GetName(), 
                                                                                                                               new GitRepoStruct("Akitaux", "Twitch"));
    }

    public class MemoryStats
    {
        public long GetKBUsage
            => Process.GetCurrentProcess().WorkingSet64 / 1024;

        public long GetMBUsage
            => GetKBUsage / 1024;

        public long GetGBUsage
            => GetMBUsage / 1024;
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