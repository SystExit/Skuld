using System;
using System.IO;

namespace Skuld.Core
{
    public static class SkuldAppContext
    {
        public static string BaseDirectory => AppContext.BaseDirectory;
        public static string TargetFrameworkName => AppContext.TargetFrameworkName;

        public static object GetData(string name) => AppContext.GetData(name);

        public static void SetSwitch(string switchName, bool isEnabled) => AppContext.SetSwitch(switchName, isEnabled);

        public static bool TryGetSwitch(string switchName, out bool isEnabled) => AppContext.TryGetSwitch(switchName, out isEnabled);

        public static string StorageDirectory = Path.Combine(BaseDirectory, "storage");

        public static string LogDirectory = Path.Combine(BaseDirectory, "logs");

        public static string TempDirectory = Path.Combine(StorageDirectory, "temp");

        public static string FontDirectory = Path.Combine(StorageDirectory, "fonts");

        public static string ProfileDirectory = Path.Combine(TempDirectory, "profile");

        public static string IMagickCache = Path.Combine(TempDirectory, "imagickCache");
    }
}