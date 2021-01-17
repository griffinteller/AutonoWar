using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Utility;
using Unity.Mathematics;

# if UNITY_EDITOR

using UnityEditor;

# endif

namespace GameTerrain
{
    [CreateAssetMenu(fileName = "QuadMeshCache", menuName = "QuadMeshCache", order = 0)]
    public class QuadMeshCache : ScriptableObject
    {
        public TextAsset cacheFile;
        public int       maxDegree;
        
        public QuadMeshCacheData Deserialize()
        {
            byte[]        bytes         = cacheFile.bytes;
            MemoryStream  memStream     = new MemoryStream(bytes);
            DeflateStream deflateStream = new DeflateStream(memStream, CompressionMode.Decompress);
            object[]      data          = (object[]) new BinaryFormatter().Deserialize(deflateStream);
            int[][] triangles = (int[][]) data[0];

            Vector2[][] uvs = Array.ConvertAll(
                (float2[][]) data[1],
                uvRow => Array.ConvertAll(
                    uvRow,
                    uv => (Vector2) uv));

            return new QuadMeshCacheData
            {
                Triangles = triangles,
                Uvs       = uvs
            };
        }

# if UNITY_EDITOR

        [Header("Generation Settings")] public string cacheFileName;

        public void Generate()
        {
            int[][]    tris    = GenerateTriangles();
            float2[][] flt2Uvs = GenerateUvs(); // uvs have to be stored as float2 since Vector2 is not serializable
            WriteToFile(tris, flt2Uvs);
        }

        private void WriteToFile(int[][] triangles, float2[][] flt2Uvs)
        {
            object[] data = {triangles, flt2Uvs};

            FileStream cacheFileStream = new FileStream(
                SystemUtility.GetAbsoluteAssetDirectory(this) + cacheFileName,
                FileMode.Create,
                FileAccess.Write);

            DeflateStream deflateStream = new DeflateStream(cacheFileStream, CompressionMode.Compress);
            new BinaryFormatter().Serialize(deflateStream, data);
            deflateStream.Close();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private int[][] GenerateTriangles()
        {
            int[][] result = new int[maxDegree + 1][];

            for (int degree = 1; degree <= maxDegree; degree++)
            {
                int   verticesPerSide     = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;
                int   edgesPerSide        = verticesPerSide                     - 1;
                int[] thisDegreeTriangles = new int[edgesPerSide * edgesPerSide * 2 * 3];

                for (int row = 0; row < verticesPerSide - 1; row++)
                for (int col = 0; col < verticesPerSide - 1; col++)
                {
                    int firstTriangleIndex = (row * edgesPerSide + col) * 2 * 3;
                    thisDegreeTriangles[firstTriangleIndex]     = row       * verticesPerSide + col;
                    thisDegreeTriangles[firstTriangleIndex + 1] = (row + 1) * verticesPerSide + col + 1;
                    thisDegreeTriangles[firstTriangleIndex + 2] = (row + 1) * verticesPerSide + col;

                    int secondTriangleIndex = firstTriangleIndex + 3;
                    thisDegreeTriangles[secondTriangleIndex]     = row       * verticesPerSide + col;
                    thisDegreeTriangles[secondTriangleIndex + 1] = row       * verticesPerSide + col + 1;
                    thisDegreeTriangles[secondTriangleIndex + 2] = (row + 1) * verticesPerSide + col + 1;
                }

                result[degree] = thisDegreeTriangles;
            }

            return result;
        }

        private float2[][] GenerateUvs()
        {
            float2[][] flt2Uvs = new float2[maxDegree + 1][];

            for (byte degree = 0; degree <= maxDegree; degree++)
            {
                int      verticesPerSide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;
                float2[] uv              = new float2[verticesPerSide * verticesPerSide];

                for (int row = 0; row < verticesPerSide; row++)
                for (int col = 0; col < verticesPerSide; col++)
                    uv[row * verticesPerSide + col] = new Vector2(
                        (float) col / (verticesPerSide - 1),
                        1 - row / (float) (verticesPerSide - 1));

                flt2Uvs[degree] = uv;
            }

            return flt2Uvs;
        }
        
# endif
    }

    public class QuadMeshCacheData
    {
        public int[][]     Triangles;
        public Vector2[][] Uvs;
    }
}