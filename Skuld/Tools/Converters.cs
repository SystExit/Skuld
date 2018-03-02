using System;
using Discord;

namespace Skuld.Extensions
{
    public static class ExtensionMethods
    {
        public static LogSeverity FromNTwitch(this NTwitch.LogSeverity logSeverity)
        {
            if (logSeverity == NTwitch.LogSeverity.Critical)
                return LogSeverity.Critical;
            if (logSeverity == NTwitch.LogSeverity.Debug)
                return LogSeverity.Debug;
            if (logSeverity == NTwitch.LogSeverity.Error)
                return LogSeverity.Error;
            if (logSeverity == NTwitch.LogSeverity.Info)
                return LogSeverity.Info;
            if (logSeverity == NTwitch.LogSeverity.Verbose)
                return LogSeverity.Verbose;
            if (logSeverity == NTwitch.LogSeverity.Warning)
                return LogSeverity.Warning;

            return LogSeverity.Verbose;
        }
    }
}
