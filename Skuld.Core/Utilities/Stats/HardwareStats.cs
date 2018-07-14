using System;
using System.Diagnostics;

namespace Skuld.Core.Utilities.Stats
{
    public class HardwareStats
    {
        public readonly CPUStats CPU;
        public readonly MemoryStats Memory;

        public HardwareStats()
        {
            CPU = new CPUStats();
            Memory = new MemoryStats();
        }
    }

    public class CPUStats
    {
        private TimeSpan StartTime;
        private double CPUTotalUsg { get; set; }

        public void Start()
        {
            StartTime = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public void Feed()
        {
            TimeSpan newCPUTIme = Process.GetCurrentProcess().TotalProcessorTime - StartTime;
            CPUTotalUsg = newCPUTIme.Ticks / (Environment.ProcessorCount * DateTime.UtcNow.Subtract(StartTime).Ticks);
        }

        public string GetCPUUsage
            => String.Format("{0:0.0}", CPUTotalUsg * 100);
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