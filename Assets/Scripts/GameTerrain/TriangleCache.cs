using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace GameTerrain
{
    [CreateAssetMenu]
    public class TriangleCache : ScriptableObject
    {
        /*
         * This is just for a normal quad. Will rename later.
         */

        public TextAsset cacheFile;
        public int[][][] Cache; // degree, mask, triangles
        public int MaxDegree => Cache.Length - 1;

        public void OnEnable()
        {
            var bytes = cacheFile.bytes;
            var memoryStream = new MemoryStream(bytes);
            Cache = (int[][][]) new BinaryFormatter().Deserialize(memoryStream);
        }
    }
}