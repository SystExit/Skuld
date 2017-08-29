using System;
using System.Threading.Tasks;
using Skuld.Tools;
using System.Collections.Generic;
using System.IO;

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
                        List<string[]> consolelines = new List<string[]>();
                        if (item.DSeverity != null)
                            consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.DSeverity.ToString() + "]", item.Message });
                        if (item.TSeverity != null)
                            consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.TSeverity.ToString() + "]", item.Message });
                        string toconsole = ConsoleUtils.PrettyLines(consolelines, 2);
                        string tolog = null;
                        if (item.Exception != null)
                        {
                            List<string[]> loglines = new List<string[]>();
                            if(item.DSeverity!=null)
                                loglines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.DSeverity.ToString() + "]", item.Message + Environment.NewLine + item.Exception });
                            if (item.TSeverity != null)
                                loglines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", item.TimeStamp), "[" + item.Source + "]", "[" + item.TSeverity.ToString() + "]", item.Message + Environment.NewLine + item.Exception });
                            tolog = ConsoleUtils.PrettyLines(loglines, 2);
                            toconsole = toconsole + " CHECK LOGS FOR MORE INFO!";
                        }
                        else { tolog = toconsole; }
                        sw.WriteLineAsync(tolog).Wait();
                        sw.FlushAsync().Wait();
                        if (item.DSeverity == Discord.LogSeverity.Critical || item.TSeverity == NTwitch.LogSeverity.Critical)
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                        if (item.DSeverity == Discord.LogSeverity.Error || item.TSeverity == NTwitch.LogSeverity.Error)
                            Console.ForegroundColor = ConsoleColor.Red;
                        if (item.DSeverity == Discord.LogSeverity.Info || item.TSeverity == NTwitch.LogSeverity.Info)
                            Console.ForegroundColor = ConsoleColor.Green;
                        if (item.DSeverity == Discord.LogSeverity.Warning ||item.TSeverity == NTwitch.LogSeverity.Warning)
                            Console.ForegroundColor = ConsoleColor.Yellow;
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

        //Start FSW
        public async static void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            await ModuleHandler.LoadSpecificModule(e.Name);
        }
        public async static void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            await ModuleHandler.UnloadSpecificModule(e.Name);
        }
        //End FSW

        //Start Twitch
        public static Task NTwitchClient_Log(NTwitch.LogMessage arg)
        {
            Logs.Add(new Models.LogMessage(arg.Source, arg.Message, arg.Level, arg.Exception));
            return Task.CompletedTask;
        }
        //End Twitch        
    }
}
