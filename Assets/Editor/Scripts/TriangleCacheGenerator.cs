using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GameTerrain;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Editor.Scripts
{
    public class TriangleCacheGenerator : EditorWindow
    {
        public int maxDegree;

        public string fileName;
        public int[][][] Cache;

        public const string CachePath = "Assets/Cache/";
        public static string AbsoluteCachePath => Application.dataPath + "/Cache/";

        [MenuItem("Window/Triangle Cache Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<TriangleCacheGenerator>();
            window.titleContent = new GUIContent("Triangle Cache Generator");
            window.Show();
        }

        public void OnGUI()
        {
            maxDegree = EditorGUILayout.IntField("Max Degree: ", maxDegree);
            fileName = EditorGUILayout.TextField(fileName);

            if (GUILayout.Button("Generate"))
            {
                Generate();
                Write();
            }
        }

        private void Write()
        {
            var file = new FileStream(AbsoluteCachePath + fileName + ".txt",
                FileMode.Create, 
                FileAccess.Write);
            new BinaryFormatter().Serialize(file, Cache);
            file.Close();

            var obj = CreateInstance<TriangleCache>();
            AssetDatabase.CreateAsset(obj, CachePath + fileName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Generate()
        {
            Cache = new int[maxDegree + 1][][];
            
            for (byte degree = 0; degree <= maxDegree; degree++)
            {
                Cache[degree] = new int[0b10000][];

                var baseTriangles = GetBaseTrianges(degree);
                for (var i = 0; i < 0b10000; i++)
                    Cache[degree][i] = baseTriangles; // TODO: Add Mipmapping. Right now am setting baseTriangles for all
            }
        }

        private int[] GetBaseTrianges(int degree)
        {
            var verticesPerSide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;
            var edgesPerSide = verticesPerSide - 1;
            var triangles = new int[edgesPerSide * edgesPerSide * 2 * 3];
            
            for (var row = 0; row < edgesPerSide; row++)
            for (var col = 0; col < edgesPerSide; col++)
            {
                var firstTriangleIndex = (row * edgesPerSide + col) * 2 * 3;
                triangles[firstTriangleIndex] = row * verticesPerSide + col;
                triangles[firstTriangleIndex + 1] = (row + 1) * verticesPerSide + col + 1;
                triangles[firstTriangleIndex + 2] = (row + 1) * verticesPerSide + col;
                
                var secondTriangleIndex = (row * edgesPerSide + col) * 2 * 3 + 3;
                triangles[secondTriangleIndex] = row * verticesPerSide + col;
                triangles[secondTriangleIndex + 1] = row * verticesPerSide + col + 1;
                triangles[secondTriangleIndex + 2] = (row + 1) * verticesPerSide + col + 1;
            }

            return triangles;
        }
    }
}