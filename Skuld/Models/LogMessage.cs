using System;
using Discord;

namespace Skuld.Models
{
    public class LogMessage
    {
        public DateTime TimeStamp { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public LogSeverity Severity { get; set; }
        public Exception Exception { get; set; }

        public LogMessage(string source, string message, LogSeverity severity, Exception exception = null)
        {
            TimeStamp = DateTime.Now;
            Source = source;
            Message = message;
            Severity = severity;
            Exception = exception;
        }
    }
}
