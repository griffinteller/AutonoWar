using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utility;
# if UNITY_EDITOR

# endif

namespace GameTerrain.old
{
    [CreateAssetMenu]
    public class UvCache : ScriptableObject
    {
        [SerializeField] private TextAsset cacheFile;
        public Vector2[][] Cache;

        public void OnEnable()
        {
            
# if UNITY_EDITOR

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            
# endif
            
            byte[] bytes = cacheFile.bytes;
            MemoryStream memoryStream = new MemoryStream(bytes);
            float2[][] floatCache = (float2[][]) new BinaryFormatter().Deserialize(memoryStream);
            
            Cache = new Vector2[floatCache.Length][];
            for (int degree = 0; degree < floatCache.Length; degree++)
            {
                Cache[degree] = new Vector2[floatCache[degree].Length];
                for (int vertex = 0; vertex < floatCache[degree].Length; vertex++)
                {
                    float2 uv = floatCache[degree][vertex];
                    Cache[degree][vertex] = new Vector2(uv.x, uv.y);
                }
            }
        }
        
# if UNITY_EDITOR

        [SerializeField] private int maxDegree;
        [SerializeField] private string fileName;

        public void GenerateCache()
        {
            float2[][] cache = GenerateUvs();
            FileStream file = new FileStream(SystemUtility.AbsoluteCachePath + fileName + ".txt",
                FileMode.Create, FileAccess.Write);
            new BinaryFormatter().Serialize(file, cache);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private float2[][] GenerateUvs()
        {
            float2[][] result = new float2[maxDegree + 1][];
            
            for (byte degree = 0; degree <= maxDegree; degree++)
            {
                int verticesPerSide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;
                float2[] uv = new float2[verticesPerSide * verticesPerSide];
                
                for (int row = 0; row < verticesPerSide; row++)
                for (int col = 0; col < verticesPerSide; col++)
                    uv[row * verticesPerSide + col] = new Vector2(
                        (float) col / (verticesPerSide - 1),
                        1 - row / (float) (verticesPerSide - 1));

                result[degree] = uv;
            }

            return result;
        }

# endif
    }
}