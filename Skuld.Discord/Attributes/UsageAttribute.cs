using System;

namespace Skuld.Discord.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsageAttribute : Attribute
    {
        public string Usage { get; private set; }

        /// <summary>
        /// Sets the usage of the command
        /// </summary>
        /// <param name="usage">Usage String</param>
        public UsageAttribute(string usage)
        {
            Usage = usage;
        }
    }
}
