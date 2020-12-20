using Unity.Mathematics;
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

        public static float4 Vec3ToFloat4(Vector3 vec3)
        {
            return new float4(vec3.x, vec3.y, vec3.z, 0);
        }

        public static Vector3 Float4ToVec3(float4 flt4)
        {
            return new Vector3(flt4.x, flt4.y, flt4.z);
        }
    }
}