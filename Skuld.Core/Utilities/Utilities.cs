using System.Runtime.CompilerServices;

namespace Skuld.Core.Utilities
{
    public static class Utils
    {
        public static string GetCaller([CallerMemberName] string caller = null)
            => caller;

        public static bool IsBitSet(this int i, int shifted)
            => (i & shifted) != 0;

        public static bool IsBitSet(this ulong i, ulong shifted)
            => (i & shifted) != 0;

        public const ulong BotCreator = 1 << 0;
        public const ulong BotAdmin = 1 << 1;
        public const ulong BotDonator = 1 << 2;
        public const ulong BotTester = 1 << 3;
        public const ulong Banned = 1 << 62;
        public const ulong NormalUser = 0;

        public const string ConfigEnvVar = "SKULD_CONFIGID";
        public const string ConStrEnvVar = "SKULD_CONNSTR";
        public const string LogLvlEnvVar = "SKULD_LOGLEVEL";
    }
}
