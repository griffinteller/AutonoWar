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

        public static string GetFormattedTime(float seconds)
        {
            return ((int) (seconds / 60)).ToString("D2") 
                + ":" 
                + (seconds % 60).ToString("00.00");
        }
    }
}