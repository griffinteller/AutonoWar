using System;

namespace Utility
{
    public static class TimeUtility
    {

        public static long CurrentTimeMillis()
        {
            
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentEpochTime = (long)(DateTime.UtcNow - epochStart).TotalMilliseconds;
            return currentEpochTime;
            
        }
        
    }
}