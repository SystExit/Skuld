using System;

namespace Skuld.Core.Models
{
    public struct EventResult : IEquatable<EventResult>
    {
        public bool Successful;
        public string Error;
        public object Data;
        public Exception Exception;

        public override bool Equals(Object obj)
        {
            return obj is EventResult && this == (EventResult)obj;
        }

        public bool Equals(EventResult obj)
        {
            return this == obj;
        }

        public static bool operator ==(EventResult x, EventResult y)
        {
            if (x.Successful == y.Successful && x.Error == y.Error && x.Data == y.Data && x.Exception == y.Exception)
            { return true; }

            return false;
        }

        public static bool operator !=(EventResult x, EventResult y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Successful.GetHashCode();
                hash = hash * 23 + Error?.GetHashCode() ?? 0;
                hash = hash * 23 + Data?.GetHashCode() ?? 0;
                hash = hash * 23 + Exception?.GetHashCode() ?? 0;
                return hash;
            }
        }

        public static EventResult FromSuccess(object data = null)
            => new EventResult
            {
                Successful = true,
                Data = data
            };
        public static EventResult FromFailure(string reason)
            => new EventResult
            {
                Successful = false,
                Error = reason
            };
        public static EventResult FromFailureException(string reason, Exception ex)
            => new EventResult
            {
                Successful = false,
                Error = reason,
                Exception = ex
            };
    }
}