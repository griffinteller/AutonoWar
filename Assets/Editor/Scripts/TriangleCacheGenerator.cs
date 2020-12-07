using System;
using System.Collections.Generic;
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
            TriangleCacheGenerator window = GetWindow<TriangleCacheGenerator>();
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
            FileStream file = new FileStream(AbsoluteCachePath + fileName + ".txt",
                FileMode.Create, 
                FileAccess.Write);
            new BinaryFormatter().Serialize(file, Cache);
            file.Close();

            TriangleCache obj = CreateInstance<TriangleCache>();
            AssetDatabase.CreateAsset(obj, CachePath + fileName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Generate()
        {
            Cache = new int[maxDegree + 1][][];
            
            for (byte degree = 1; degree <= maxDegree; degree++)
            {
                Cache[degree] = new int[0b10000][];

                int[] innerTriangles = GetInnerTrianges(degree);

                for (int i = 0; i < 0b10000; i++)
                {
                    int verticesPerSide = (int) (Math.Pow(2, degree) + 0.5f) + 1;
                    int[] topTriangles = GetTopTriangles((i & 1) == 1, verticesPerSide);
                    int[] rightTriangles = GetRightTriangles((i & 2) == 2, verticesPerSide);
                    int[] bottomTriangles = GetBottomTriangles((i & 4) == 4, verticesPerSide);
                    int[] leftTriangles = GetLeftTriangles((i & 8) == 8, verticesPerSide);

                    int totalTriangles = innerTriangles.Length
                                         + topTriangles.Length
                                         + rightTriangles.Length
                                         + bottomTriangles.Length
                                         + leftTriangles.Length;
                    
                    int[] allTriangles = new int[totalTriangles];
                    innerTriangles.CopyTo(allTriangles, 0);
                    topTriangles.CopyTo(allTriangles, innerTriangles.Length);
                    rightTriangles.CopyTo(allTriangles, innerTriangles.Length + topTriangles.Length);
                    bottomTriangles.CopyTo(allTriangles, 
                        innerTriangles.Length + topTriangles.Length + rightTriangles.Length);
                    leftTriangles.CopyTo(allTriangles, 
                        innerTriangles.Length + topTriangles.Length + rightTriangles.Length 
                        + bottomTriangles.Length);

                    Cache[degree][i] = allTriangles;
                }
            }
        }

        private int[] GetInnerTrianges(int degree)
        {
            int verticesPerSide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;
            int edgesPerSide = verticesPerSide - 1;
            int[] triangles = new int[edgesPerSide * edgesPerSide * 2 * 3];
            
            for (int row = 1; row < edgesPerSide - 1; row++)
            for (int col = 1; col < edgesPerSide - 1; col++)
            {
                int firstTriangleIndex = (row * edgesPerSide + col) * 2 * 3;
                triangles[firstTriangleIndex] = row * verticesPerSide + col;
                triangles[firstTriangleIndex + 1] = (row + 1) * verticesPerSide + col + 1;
                triangles[firstTriangleIndex + 2] = (row + 1) * verticesPerSide + col;
                
                int secondTriangleIndex = (row * edgesPerSide + col) * 2 * 3 + 3;
                triangles[secondTriangleIndex] = row * verticesPerSide + col;
                triangles[secondTriangleIndex + 1] = row * verticesPerSide + col + 1;
                triangles[secondTriangleIndex + 2] = (row + 1) * verticesPerSide + col + 1;
            }

            return triangles;
        }

        private int[] GetTopTriangles(bool mipmapped, int verticesPerSide)
        {
            int edgesPerSide = verticesPerSide - 1;
            
            int[] triangles = new int[
                mipmapped ? 
                    (3 * edgesPerSide / 2  - 2) * 3 
                    : (2 * edgesPerSide - 2) * 3];

            if (mipmapped)
            {
                // big triangles
                for (int col = 0; col < verticesPerSide - 2; col += 2)
                {
                    int triangleIndex = col / 2 * 3;

                    triangles[triangleIndex] = col;
                    triangles[triangleIndex + 1] = col + 2;
                    triangles[triangleIndex + 2] = verticesPerSide + col + 1;
                }
                
                // small triangles
                for (int col = 2; col < verticesPerSide - 2; col += 2)
                {
                    int firstTriangleIndex = (edgesPerSide / 2 + col - 2) * 3;

                    triangles[firstTriangleIndex] = col;
                    triangles[firstTriangleIndex + 1] = verticesPerSide + col;
                    triangles[firstTriangleIndex + 2] = verticesPerSide + col - 1;

                    int secondTriangleIndex = firstTriangleIndex + 3;

                    triangles[secondTriangleIndex] = col;
                    triangles[secondTriangleIndex + 1] = verticesPerSide + col + 1;
                    triangles[secondTriangleIndex + 2] = verticesPerSide + col;
                }
            }
            else
            {
                // inner triangles
                int firstTriangleIndex;
                int secondTriangleIndex;
                for (int col = 1; col < verticesPerSide - 2; col++)
                {
                    firstTriangleIndex = (col - 1) * 2 * 3;
                    triangles[firstTriangleIndex] = col;
                    triangles[firstTriangleIndex + 1] = verticesPerSide + col + 1;
                    triangles[firstTriangleIndex + 2] = verticesPerSide + col;
                
                    secondTriangleIndex = firstTriangleIndex + 3;
                    triangles[secondTriangleIndex] = col;
                    triangles[secondTriangleIndex + 1] = col + 1;
                    triangles[secondTriangleIndex + 2] = verticesPerSide + col + 1;
                }

                firstTriangleIndex = (edgesPerSide - 2) * 2 * 3;
                triangles[firstTriangleIndex] = 0;
                triangles[firstTriangleIndex + 1] = 1;
                triangles[firstTriangleIndex + 2] = verticesPerSide + 1;

                secondTriangleIndex = firstTriangleIndex + 3;
                triangles[secondTriangleIndex] = verticesPerSide - 2;
                triangles[secondTriangleIndex + 1] = verticesPerSide - 1;
                triangles[secondTriangleIndex + 2] = 2 * verticesPerSide - 2;
            }

            return triangles;
        }
        
        private int[] GetRightTriangles(bool mipmapped, int verticesPerSide)
        {
            int edgesPerSide = verticesPerSide - 1;
            
            int[] triangles = new int[
                mipmapped ? 
                    (3 * edgesPerSide / 2  - 2) * 3 
                    : (2 * edgesPerSide - 2) * 3];

            if (mipmapped)
            {
                // big triangles
                for (int row = 0; row < verticesPerSide - 2; row += 2)
                {
                    int triangleIndex = row / 2 * 3;

                    triangles[triangleIndex] = row * verticesPerSide + verticesPerSide - 1;
                    triangles[triangleIndex + 1] = (row + 2) * verticesPerSide + verticesPerSide - 1;
                    triangles[triangleIndex + 2] = (row + 1) * verticesPerSide + verticesPerSide - 2;
                }
                
                // small triangles
                for (int row = 2; row < verticesPerSide - 2; row += 2)
                {
                    int firstTriangleIndex = (edgesPerSide / 2 + row - 2) * 3;

                    triangles[firstTriangleIndex] = row * verticesPerSide + verticesPerSide - 1;
                    triangles[firstTriangleIndex + 1] = row * verticesPerSide + verticesPerSide - 2;
                    triangles[firstTriangleIndex + 2] = (row - 1) * verticesPerSide + verticesPerSide - 2;

                    int secondTriangleIndex = firstTriangleIndex + 3;

                    triangles[secondTriangleIndex] = row * verticesPerSide + verticesPerSide - 1;
                    triangles[secondTriangleIndex + 1] = (row + 1) * verticesPerSide + verticesPerSide - 2;
                    triangles[secondTriangleIndex + 2] = row * verticesPerSide + verticesPerSide - 2;
                }
            }
            else
            {
                // inner triangles
                int firstTriangleIndex;
                int secondTriangleIndex;
                for (int row = 1; row < verticesPerSide - 2; row++)
                {
                    firstTriangleIndex = (row - 1) * 2 * 3;
                    triangles[firstTriangleIndex] = row * verticesPerSide + verticesPerSide - 2;
                    triangles[firstTriangleIndex + 1] = (row + 1) * verticesPerSide + verticesPerSide - 1;
                    triangles[firstTriangleIndex + 2] = (row + 1) * verticesPerSide + verticesPerSide - 2;
                
                    secondTriangleIndex = firstTriangleIndex + 3;
                    triangles[secondTriangleIndex] = row * verticesPerSide + verticesPerSide - 2;
                    triangles[secondTriangleIndex + 1] = row * verticesPerSide + verticesPerSide - 1;
                    triangles[secondTriangleIndex + 2] = (row + 1) * verticesPerSide + verticesPerSide - 1;
                }

                firstTriangleIndex = (edgesPerSide - 2) * 2 * 3;
                triangles[firstTriangleIndex] = verticesPerSide - 1;
                triangles[firstTriangleIndex + 1] = 2 * verticesPerSide - 1;
                triangles[firstTriangleIndex + 2] = 2 * verticesPerSide - 2;

                secondTriangleIndex = firstTriangleIndex + 3;
                triangles[secondTriangleIndex] = verticesPerSide * (verticesPerSide - 1) - 2;
                triangles[secondTriangleIndex + 1] = verticesPerSide * (verticesPerSide - 1) - 1;
                triangles[secondTriangleIndex + 2] = verticesPerSide * verticesPerSide - 1;
            }

            return triangles;
        }
        
        private int[] GetBottomTriangles(bool mipmapped, int verticesPerSide)
        {
            int edgesPerSide = verticesPerSide - 1;
            
            int[] triangles = new int[
                mipmapped ? 
                    (3 * edgesPerSide / 2  - 2) * 3 
                    : (2 * edgesPerSide - 2) * 3];

            int penUltRowStart = verticesPerSide * (verticesPerSide - 2);
            
            if (mipmapped)
            {
                // big triangles
                for (int col = 0; col < verticesPerSide - 2; col += 2)
                {
                    int triangleIndex = col / 2 * 3;

                    triangles[triangleIndex] = penUltRowStart + col + 1;
                    triangles[triangleIndex + 1] = penUltRowStart + verticesPerSide + col + 2;
                    triangles[triangleIndex + 2] = penUltRowStart + verticesPerSide + col;
                }
                
                // small triangles
                for (int col = 2; col < verticesPerSide - 2; col += 2)
                {
                    int firstTriangleIndex = (edgesPerSide / 2 + col - 2) * 3;

                    triangles[firstTriangleIndex] = penUltRowStart + col;
                    triangles[firstTriangleIndex + 1] = penUltRowStart + verticesPerSide + col;
                    triangles[firstTriangleIndex + 2] = penUltRowStart + col - 1;

                    int secondTriangleIndex = firstTriangleIndex + 3;

                    triangles[secondTriangleIndex] = penUltRowStart + col;
                    triangles[secondTriangleIndex + 1] = penUltRowStart + col + 1;
                    triangles[secondTriangleIndex + 2] = penUltRowStart + verticesPerSide + col;
                }
            }
            else
            {
                // inner triangles
                int firstTriangleIndex;
                int secondTriangleIndex;
                for (int col = 1; col < verticesPerSide - 2; col++)
                {
                    firstTriangleIndex = (col - 1) * 2 * 3;
                    triangles[firstTriangleIndex] = penUltRowStart + col;
                    triangles[firstTriangleIndex + 1] = penUltRowStart + verticesPerSide + col + 1;
                    triangles[firstTriangleIndex + 2] = penUltRowStart + verticesPerSide + col;
                
                    secondTriangleIndex = firstTriangleIndex + 3;
                    triangles[secondTriangleIndex] = penUltRowStart + col;
                    triangles[secondTriangleIndex + 1] = penUltRowStart + col + 1;
                    triangles[secondTriangleIndex + 2] = penUltRowStart + verticesPerSide + col + 1;
                }

                firstTriangleIndex = (edgesPerSide - 2) * 2 * 3;
                triangles[firstTriangleIndex] = penUltRowStart + 1;
                triangles[firstTriangleIndex + 1] = penUltRowStart + verticesPerSide + 1;
                triangles[firstTriangleIndex + 2] = penUltRowStart + verticesPerSide;

                secondTriangleIndex = firstTriangleIndex + 3;
                triangles[secondTriangleIndex] = penUltRowStart + verticesPerSide - 2;
                triangles[secondTriangleIndex + 1] = verticesPerSide * verticesPerSide - 1;
                triangles[secondTriangleIndex + 2] = verticesPerSide * verticesPerSide - 2;
            }

            return triangles;
        }
        
        private int[] GetLeftTriangles(bool mipmapped, int verticesPerSide)
        {
            int edgesPerSide = verticesPerSide - 1;
            
            int[] triangles = new int[
                mipmapped ? 
                    (3 * edgesPerSide / 2  - 2) * 3 
                    : (2 * edgesPerSide - 2) * 3];

            if (mipmapped)
            {
                // big triangles
                for (int row = 0; row < verticesPerSide - 2; row += 2)
                {
                    int triangleIndex = row / 2 * 3;

                    triangles[triangleIndex] = row * verticesPerSide;
                    triangles[triangleIndex + 1] = (row + 1) * verticesPerSide + 1;
                    triangles[triangleIndex + 2] = (row + 2) * verticesPerSide;
                }
                
                // small triangles
                for (int row = 2; row < verticesPerSide - 2; row += 2)
                {
                    int firstTriangleIndex = (edgesPerSide / 2 + row - 2) * 3;

                    triangles[firstTriangleIndex] = row * verticesPerSide + 1;
                    triangles[firstTriangleIndex + 1] = row * verticesPerSide;
                    triangles[firstTriangleIndex + 2] = (row - 1) * verticesPerSide + 1;

                    int secondTriangleIndex = firstTriangleIndex + 3;

                    triangles[secondTriangleIndex] = row * verticesPerSide + 1;
                    triangles[secondTriangleIndex + 1] = (row + 1) * verticesPerSide + 1;
                    triangles[secondTriangleIndex + 2] = row * verticesPerSide;
                }
            }
            else
            {
                // inner triangles
                int firstTriangleIndex;
                int secondTriangleIndex;
                for (int row = 1; row < verticesPerSide - 2; row++)
                {
                    firstTriangleIndex = (row - 1) * 2 * 3;
                    triangles[firstTriangleIndex] = row * verticesPerSide;
                    triangles[firstTriangleIndex + 1] = (row + 1) * verticesPerSide + 1;
                    triangles[firstTriangleIndex + 2] = (row + 1) * verticesPerSide;
                
                    secondTriangleIndex = firstTriangleIndex + 3;
                    triangles[secondTriangleIndex] = row * verticesPerSide;
                    triangles[secondTriangleIndex + 1] = row * verticesPerSide + 1;
                    triangles[secondTriangleIndex + 2] = (row + 1) * verticesPerSide + 1;
                }

                firstTriangleIndex = (edgesPerSide - 2) * 2 * 3;
                triangles[firstTriangleIndex] = 0;
                triangles[firstTriangleIndex + 1] = verticesPerSide + 1;
                triangles[firstTriangleIndex + 2] = verticesPerSide;

                secondTriangleIndex = firstTriangleIndex + 3;
                triangles[secondTriangleIndex] = verticesPerSide * (verticesPerSide - 2);
                triangles[secondTriangleIndex + 1] = verticesPerSide * (verticesPerSide - 2) + 1;
                triangles[secondTriangleIndex + 2] = verticesPerSide * (verticesPerSide - 1);
            }

            return triangles;
        }
    }
}