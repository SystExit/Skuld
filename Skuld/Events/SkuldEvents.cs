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
        public static Task Bot_Log(Discord.LogMessage arg)
        {
            Logger.AddToLogs(new Models.LogMessage(arg.Source, arg.Message, arg.Severity, arg.Exception));
            return Task.CompletedTask;
        }
        //End logging
        
        //Start Twitch
        public static Task NTwitchClient_Log(NTwitch.LogMessage arg)
        {
            Logger.AddToLogs(new Models.LogMessage(arg.Source, arg.Message, (arg.Level).FromNTwitch(), arg.Exception));
            return Task.CompletedTask;
        }
        //End Twitch
    }
}
