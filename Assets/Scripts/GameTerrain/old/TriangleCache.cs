using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Utility;
# if UNITY_EDITOR

# endif

namespace GameTerrain.old
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

        public TextAsset            cacheFile;
        public NativeArray<int>[][] Cache; // degree, mask, triangles
        public int                  MaxDegree => Cache.Length - 1;

        public void OnEnable()
        {
            
# if UNITY_EDITOR

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            
# endif
            
            byte[] bytes = cacheFile.bytes;
            MemoryStream memoryStream = new MemoryStream(bytes);
            int[][][] cacheManagedArray = (int[][][]) new BinaryFormatter().Deserialize(memoryStream);
            Cache = BuildCache(cacheManagedArray); 
        }

        private NativeArray<int>[][] BuildCache(int[][][] cacheManagedArray)
        {
            NativeArray<int>[][] result = new NativeArray<int>[cacheManagedArray.Length][];

            for (int degree = 1; degree < cacheManagedArray.Length; degree++)
            {
                result[degree] = new NativeArray<int>[cacheManagedArray[degree].Length];
                for (int mask = 0; mask < cacheManagedArray[degree].Length; mask++)
                {
                    result[degree][mask] = new NativeArray<int>(
                        cacheManagedArray[degree][mask],
                        Allocator.Persistent);
                }
            }

            Debug.Log(result[6][0][0] + " " + result[6][0][1] + " " + result[6][0][2]);
            return result;
        }
        
        public void DisposeNativeArrays()
        {
            foreach (NativeArray<int>[] arr1 in Cache)
            {
                if (arr1 == null) // no 0th degree cache
                    continue;
                
                foreach (NativeArray<int> arr2 in arr1)
                    arr2.TryDispose();
            }
        }
    }
}
    
    