using System;
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

        /// <summary>
        /// Gets the experience multiplier from Users Minutes in Voice
        /// </summary>
        /// <param name="expIndeterminate">Indeterminate Value for parabola</param>
        /// <param name="minMinutes">Minimum Minutes In voice</param>
        /// <param name="maxExperience">Maximum XP to grant</param>
        /// <param name="timeInVoice">Users time in voice by minutes</param>
        /// <returns></returns>
        public static int GetExpMultiFromMinutesInVoice(float expIndeterminate, ulong minMinutes, ulong maxExperience, ulong timeInVoice)
        {
            if (timeInVoice < minMinutes)
                return 0; //if less than minimum minutes return 0 multiplier

           var result = Math.Pow(expIndeterminate * (timeInVoice - minMinutes), 2); //do math

            if (result > maxExperience)
                result = maxExperience; //clamp to 100 as limit

            if (result < 0)
                result = 0; //if negative clamp to zero

            return (int)Math.Round(result); //return rounded integral version of result
        }
    }
}
