using System.Diagnostics;

namespace Skuld.Core.Utilities.Stats
{
    public class HardwareStats
    {
        public readonly MemoryStats Memory;

        public HardwareStats()
        {
            Memory = new MemoryStats();
        }
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