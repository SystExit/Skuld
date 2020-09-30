using System;

namespace Skuld.Bot.Discord.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsageAttribute : Attribute
    {
        public string[] Usage { get; private set; }

        /// <summary>
        /// Sets the usage of the command
        /// </summary>
        /// <param name="usage">Usage String</param>
        public UsageAttribute(params string[] usage)
        {
            Usage = usage;
        }
    }
}