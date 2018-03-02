using System;
using System.Threading.Tasks;
using Skuld.Tools;
using System.Collections.Generic;
using System.Linq;
using Skuld.Extensions;

namespace Skuld.Events
{
    public class SkuldEvents : Bot
    {
        //Start logging
        public static async Task Bot_Log(Discord.LogMessage arg)
        {
            await Logger.AddToLogs(new Models.LogMessage(arg.Source, arg.Message, arg.Severity, arg.Exception));
        }
        //End logging
        
        //Start Twitch
        public static async Task NTwitchClient_Log(NTwitch.LogMessage arg)
        {
            await Logger.AddToLogs(new Models.LogMessage(arg.Source, arg.Message, (arg.Level).FromNTwitch(), arg.Exception));
        }
        //End Twitch
    }
}
