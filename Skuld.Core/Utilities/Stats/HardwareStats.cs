using System.Diagnostics;

namespace Skuld.Core.Utilities.Stats
{
    public static class HardwareStats
    {
        public static readonly MemoryStats Memory = new MemoryStats();
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
}