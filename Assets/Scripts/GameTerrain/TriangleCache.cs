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
         *
         * Masking:
         *
         * Top    - 0b0001
         * Right  - 0b0010
         * Bottom - 0b0100
         * Left   - 0b1000
         */

        public TextAsset cacheFile;
        public int[][][] Cache; // degree, mask, triangles
        public int MaxDegree => Cache.Length - 1;

        public void OnEnable()
        {
            byte[] bytes = cacheFile.bytes;
            MemoryStream memoryStream = new MemoryStream(bytes);
            Cache = (int[][][]) new BinaryFormatter().Deserialize(memoryStream);
        }
    }
}