using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility;

# if UNITY_EDITOR

using UnityEditor;

# endif

namespace GameTerrain
{
    [CreateAssetMenu]
    public class QuadTerrainHeightmap : ScriptableObject
    {
        
        /*
         *
         * TODO: add ability to create separate cached ScriptObjs that are loaded when needed
         * 
         */
        public TextAsset  storageFile;
        public Vector2Int quadDensity;
        public byte       maxDegree;
        public Texture2D  rawHeightmap;
        public Vector2Int originPos;
        public float      maxPossibleHeight;
        public string     storageFileName;

        public NativeArray<float> Heights; // longitude, latitude, row, col

        private int _verticesPerSide;
        private int _verticesPerQuad;
        private int _verticesPerLongitude;

        public void OnEnable()
        {
            # if UNITY_EDITOR

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("Not playing! Cancelling heightmap load...");

                return;
            }
            
            # endif

            byte[]        bytes         = storageFile.bytes;
            MemoryStream  memStream     = new MemoryStream(bytes);
            DeflateStream deflateStream = new DeflateStream(memStream, CompressionMode.Decompress);
            float[]       data          = (float[]) new BinaryFormatter().Deserialize(deflateStream);
            Heights = new NativeArray<float>(data, Allocator.Persistent);
        }

        public void DisposeNativeArrays()
        {
            Heights.TryDispose();
        }

# if UNITY_EDITOR
        
        public void Awake()
        {
            _verticesPerSide = (int) (Mathf.Pow(2, maxDegree) + 0.5f) + 1;
            _verticesPerQuad = _verticesPerSide * _verticesPerSide;
            _verticesPerLongitude = _verticesPerQuad * quadDensity.y;
        }

        public void Generate()
        {
            Awake();
            
            Texture2D stretchedHeightmap = rawHeightmap; // TODO: add support for automatically stretching textures
            int pixelsWide = stretchedHeightmap.width;
            int pixelsHeight = stretchedHeightmap.height;
            
            Debug.Log(pixelsWide + " " + pixelsHeight);
            
            float[] heights = new float[quadDensity.x * _verticesPerLongitude];

            for (int longitude = 0; longitude < quadDensity.x; longitude++)
            for (int latitude = 0; latitude < quadDensity.y; latitude++)
            for (int row = 0; row < _verticesPerSide; row++)
            for (int col = 0; col < _verticesPerSide; col++)
            {
                int pixelX = (longitude * (_verticesPerSide - 1) + col - _verticesPerSide / 2 + originPos.x +
                              pixelsWide) % pixelsWide;
                int pixelY = (latitude * (_verticesPerSide - 1) - row + _verticesPerSide / 2 - originPos.y +
                              pixelsHeight) % pixelsHeight;
                heights[longitude * _verticesPerLongitude
                        + latitude * _verticesPerQuad
                        + row * _verticesPerSide
                        + col] = stretchedHeightmap.GetPixel(pixelX, pixelY).r * maxPossibleHeight;
            }

            string storageAssetPath = SystemUtility.GetAssetDirectory(this) + storageFileName;
            FileStream storageStream = new FileStream(
                SystemUtility.AbsoluteProjectPath + storageAssetPath,
                FileMode.Create,
                FileAccess.Write);

            DeflateStream deflateStream = new DeflateStream(storageStream, CompressionMode.Compress);
            new BinaryFormatter().Serialize(deflateStream, heights);
            deflateStream.Close();
            
            AssetDatabase.Refresh();

            storageFile = AssetDatabase.LoadAssetAtPath<TextAsset>(storageAssetPath);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

# endif
        
    }
}