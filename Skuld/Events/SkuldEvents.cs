using System;
using System.Threading.Tasks;
using Skuld.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Skuld.Events
{
    public class SkuldEvents : Bot
    {
        //Start logging
        public async static void Logs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                try
                {
                    foreach (Models.LogMessage item in e.NewItems)
                    {
                        string source = String.Join("",item.Source.Take(1));
                        source+=String.Join("",item.Source.Reverse().Take(3).Reverse());
                        var consolelines = new List<string[]>();
                        if (item.DSeverity != null)
                            consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + source + "]", "[" + item.DSeverity.ToString()[0] + "]", item.Message });
                        if (item.TSeverity != null)
                            consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + source + "]", "[" + item.TSeverity.ToString()[0] + "]", item.Message });
                        string toconsole = ConsoleUtils.PrettyLines(consolelines, 2);
                        string tolog = null;
                        if (item.Exception != null)
                        {
                            var loglines = new List<string[]>();
                            if(item.DSeverity!=null)
                                loglines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.DSeverity + "]", item.Message + Environment.NewLine + item.Exception });
                            if (item.TSeverity != null)
                                loglines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.TSeverity + "]", item.Message + Environment.NewLine + item.Exception });
                            tolog = ConsoleUtils.PrettyLines(loglines, 2);
                            toconsole = toconsole + " CHECK LOGS FOR MORE INFO!";
                        }
                        else { tolog = ConsoleUtils.PrettyLines(new List<string[]>() { new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.DSeverity.ToString()[0] + "]", item.Message } }, 2); }
                        sw.WriteLineAsync(tolog).Wait();
                        sw.FlushAsync().Wait();
                        if (item.DSeverity == Discord.LogSeverity.Critical || item.TSeverity == NTwitch.LogSeverity.Critical)
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                        if (item.DSeverity == Discord.LogSeverity.Error || item.TSeverity == NTwitch.LogSeverity.Error)
                            Console.ForegroundColor = ConsoleColor.Red;
                        if (item.DSeverity == Discord.LogSeverity.Info || item.TSeverity == NTwitch.LogSeverity.Info)
                            Console.ForegroundColor = ConsoleColor.Green;
                        if (item.DSeverity == Discord.LogSeverity.Warning || item.TSeverity == NTwitch.LogSeverity.Warning)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        if (item.DSeverity == Discord.LogSeverity.Verbose || item.TSeverity == NTwitch.LogSeverity.Verbose)
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Out.WriteLineAsync(toconsole).Wait();
                        Console.ForegroundColor = ConsoleColor.White;
                        await Task.Delay(100);
                    }
                }
                catch
                {

                }
            }
        }
        public static Task Bot_Log(Discord.LogMessage arg)
        {
            Logs.Add(new Models.LogMessage(arg.Source, arg.Message, arg.Severity, arg.Exception));
            return Task.CompletedTask;
        }
        //End logging
        
        //Start Twitch
        public static Task NTwitchClient_Log(NTwitch.LogMessage arg)
        {
            Logs.Add(new Models.LogMessage(arg.Source, arg.Message, arg.Level, arg.Exception));
            return Task.CompletedTask;
        }
        //End Twitch        
        //AimlBot
        public static void ChatService_WrittenToLog()
        {
            Logs.Add(new Models.LogMessage("ChtSrvc", ChatService.LastLogMessage, Discord.LogSeverity.Info));
        }
    }
}
