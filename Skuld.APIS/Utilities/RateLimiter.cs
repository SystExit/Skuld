using System;

namespace Skuld.APIS.Utilities
{
    public class RateLimiter
    {
        public const int DefaultRequestLimit = 5000;
        public const int DefaultResetInterval = 30; // Minutes
        public int RequestLimit { get; private set; }
        public int RequestsRemaining { get; private set; }
        public TimeSpan ResetInterval => TimeSpan.FromMinutes(_resetInterval);
        public DateTime ResetAt { get; private set; }

        private int _resetInterval;

        public RateLimiter(int? requestLimit = null, int? resetInterval = null)
        {
            RequestLimit = requestLimit ?? DefaultRequestLimit;
            _resetInterval = resetInterval ?? DefaultResetInterval;
            ResetAt = DateTime.MinValue;
        }

        public void SetRequestLimit(int? requestLimit = null)
        {
            RequestLimit = requestLimit ?? DefaultRequestLimit;
        }
        public void SetRequestInterval(int? resetInterval = null)
        {
            _resetInterval = resetInterval ?? DefaultResetInterval;
        }

        public bool IsRatelimited()
        {
            if (ResetAt <= DateTime.UtcNow)
            {
                ResetAt = DateTime.UtcNow.AddMinutes(_resetInterval);
                RequestsRemaining = RequestLimit;
            }

            if (RequestsRemaining == 0) return true;
            RequestsRemaining--;
            return false;
        }
    }
}
