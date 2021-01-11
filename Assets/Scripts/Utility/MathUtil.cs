using System.Collections.Generic;
using Unity.Collections;
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

        public static void FromFloat4s(this NativeArray<Vector3> vector3s, NativeArray<float4> float4s)
        {
            for (int i = 0; i < vector3s.Length; i++)
                vector3s[i] = float4s[i].xyz;
        }
        
        public static void FromVector3s(this NativeArray<float4> float4s, NativeArray<Vector3> vector3s)
        {
            for (int i = 0; i < float4s.Length; i++)
                float4s[i] = (Vector4) vector3s[i];
        }

        public static bool All(this bool4 b4)
        {
            return b4.x && b4.y && b4.z && b4.w;
        }
        
        public static bool Any(this bool4 b4)
        {
            return b4.x || b4.y || b4.z || b4.w;
        }
    }
}