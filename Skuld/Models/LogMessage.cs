using System;

namespace Skuld.Models
{
    public class LogMessage
    {
        public DateTime TimeStamp { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public Discord.LogSeverity? DSeverity { get; set; }
        public NTwitch.LogSeverity? TSeverity { get; set; }
        public Exception Exception { get; set; }
        public LogMessage(string source, string message, Discord.LogSeverity severity, Exception exception = null)
        {
            TimeStamp = DateTime.Now;
            Source = source;
            Message = message;
            DSeverity = severity;
            TSeverity = null;
            Exception = exception;
        }
        public LogMessage(string source, string message, NTwitch.LogSeverity severity, Exception exception = null)
        {
            TimeStamp = DateTime.Now;
            Source = source;
            Message = message;
            DSeverity = null;
            TSeverity = severity;
            Exception = exception;
        }
    }
}
