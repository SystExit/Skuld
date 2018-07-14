using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Core.Services
{
    public class GenericLogger
    {
        public StreamWriter sw;
        private List<LogMessage> Logs;
        private readonly bool Console;
        private readonly bool File;
        private readonly Random random = new Random();

        public GenericLogger(string logfile)
        {
            Logs = new List<LogMessage>();
            Console = false;
            File = true;
            sw = new StreamWriter(logfile, true, Encoding.Unicode)
            {
                AutoFlush = true
            };
        }

        public GenericLogger(bool OutputToConsole, bool OutputToFile, string logfile)
        {
            Logs = new List<LogMessage>();
            Console = OutputToConsole;
            File = OutputToFile;
            sw = new StreamWriter(logfile, true, Encoding.Unicode)
            {
                AutoFlush = true
            };
        }

        public async Task AddToLogsAsync(LogMessage message)
        {
            if (message.Severity == Discord.LogSeverity.Verbose) return;

            Logs.Add(message);

            string tolog = null;
            string ConsoleMessage = null;
            string FileMessage = null;

            if (message.Exception != null)
            {
                var loglines = new List<string[]>
                {
                    new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity + "]", message.Message + Environment.NewLine + message.Exception }
                };
                if (Console && !File)
                {
                    ConsoleMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if (Console && File)
                {
                    ConsoleMessage = " CHECK LOGS FOR MORE INFO!";
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if (!Console && File)
                {
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if (!Console && !File)
                { }
            }

            switch (message.Severity)
            {
                case Discord.LogSeverity.Info:
                    DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}", "info");
                    break;

                case Discord.LogSeverity.Warning:
                    DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}", "warning");
                    break;

                case Discord.LogSeverity.Critical:
                    DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}\n{message.Exception}", "critical");
                    break;

                case Discord.LogSeverity.Error:
                    DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}\n{message.Exception}", "error");
                    break;
            }

            if (Console)
            {
                System.Console.ForegroundColor = message.Severity.SeverityToColor();
                var consolelines = new List<string[]>();
                if (ConsoleMessage != null)
                {
                    if (ConsoleMessage.StartsWith(" "))
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + ConsoleMessage });
                    }
                    else
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + Environment.NewLine + ConsoleMessage });
                    }
                }
                else
                {
                    consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message });
                }
                string toconsole = ConsoleUtils.PrettyLines(consolelines, 2);
                await System.Console.Out.WriteLineAsync(toconsole).ConfigureAwait(false);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
            if (File)
            {
                if (FileMessage != null)
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