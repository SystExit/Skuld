using System;
using System.Collections.Generic;
using System.Text;
using Skuld.Models;
using System.Threading.Tasks;
using Skuld.Tools;
using System.Linq;
using System.IO;

namespace Skuld.Tools
{
    public class LoggingService
    {
        public StreamWriter sw;
        List<LogMessage> Logs;
        bool Console;
        bool File;

        public LoggingService(string logfile)
        {
            Logs = new List<LogMessage>();
            Console = false;
            File = false;
			sw = new StreamWriter(logfile, true, Encoding.Unicode)
			{
				AutoFlush = true
			};
		}

        public LoggingService(bool OutputToConsole, bool OutputToFile, string logfile)
        {
            Logs = new List<LogMessage>();
            Console = OutputToConsole;
            File = OutputToFile;
			sw = new StreamWriter(logfile, true, Encoding.Unicode)
			{
				AutoFlush = true
			};
		}

        public async Task AddToLogs(LogMessage message)
        {
            Logs.Add(message);
            string source = String.Join("", message.Source.Take(1));
            source += String.Join("", message.Source.Reverse().Take(3).Reverse());

            string tolog = null;
            string ConsoleMessage = null,
                FileMessage = null;
			
            if(message.Exception!=null)
            {
                var loglines = new List<string[]>
                {
                    new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity + "]", message.Message + Environment.NewLine + message.Exception }
                };
                if (Console && !File)
                {
                    ConsoleMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(Console && File)
                {
                    ConsoleMessage = " CHECK LOGS FOR MORE INFO!";
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(!Console && File)
                {
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(!Console && !File)
                { }
            }

            if (Console)
            {
                System.Console.ForegroundColor = Tools.ColorBasedOnSeverity(message.Severity);
                var consolelines = new List<string[]>();
                if(ConsoleMessage!=null)
                {
                    if (ConsoleMessage.StartsWith(" "))
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + ConsoleMessage });
                    }
                    else
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + Environment.NewLine + ConsoleMessage });
                    }
                }
                else
                {
                    consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message });
                }
                string toconsole = ConsoleUtils.PrettyLines(consolelines, 2);
                await System.Console.Out.WriteLineAsync(toconsole);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
            if(File)
            {
                if(FileMessage!=null)
                {
                    tolog = ConsoleUtils.PrettyLines(new List<string[]> { new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + Environment.NewLine + FileMessage } }, 2);
                }
                else
                {
                    tolog = ConsoleUtils.PrettyLines(new List<string[]> { new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message } }, 2);
                }

                sw.WriteLine(tolog);
            }
        }
    }
}
