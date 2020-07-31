using UnityEngine;

namespace Utility
{
    public static class MathUtil
    {
        public static float[] Linspace(float start, float end, int count, bool endIsInculsive = true)
        {
            var result = new float[count];

            float commonDifference;
            if (endIsInculsive)
                commonDifference = (end - start) / (count - 1);
            else
                commonDifference = (end - start) / count;

            for (var i = 0; i < count; i++)
                result[i] = start + commonDifference * i;

            return result;
        }
    }
}