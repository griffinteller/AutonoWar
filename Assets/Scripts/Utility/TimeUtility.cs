using System;

namespace Utility
{
    public static class TimeUtility
    {
        public static readonly DateTime EpochStart =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeSeconds()
        {
            var currentEpochTime = (long) (DateTime.UtcNow - EpochStart).TotalSeconds;
            return currentEpochTime;
        }
    }
}