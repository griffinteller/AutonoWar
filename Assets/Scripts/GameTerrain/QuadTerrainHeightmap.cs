# if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using Utility;

namespace GameTerrain
{

    [CreateAssetMenu(fileName = "New Heightmap", menuName = "Quad Terrain Heightmap", order = 0)]
    public class QuadTerrainHeightmap : ScriptableObject
    {
        public TextAsset  storageFile;
        public Vector2Int quadDensity;
        public byte       maxDegree;
        public Texture2D  rawHeightmap;
        public Vector2    offset;
        public float      maxPossibleHeight;
        public string     storageFileName;

        private int _verticesPerSide;
        private int _verticesPerQuad;
        private int _verticesPerLongitude;

        public float[] LoadHeightmap()
        {
            byte[]        bytes         = storageFile.bytes;
            MemoryStream  memStream     = new MemoryStream(bytes);
            DeflateStream deflateStream = new DeflateStream(memStream, CompressionMode.Decompress);
            float[]       data          = (float[]) new BinaryFormatter().Deserialize(deflateStream);
            
            Debug.Log(data);
            Debug.Log(quadDensity * maxDegree);

            return data;
        }

        public void Awake()
        {
            _verticesPerSide = (int) (Mathf.Pow(2, maxDegree) + 0.5f) + 1;
            _verticesPerQuad = _verticesPerSide * _verticesPerSide;
            _verticesPerLongitude = _verticesPerQuad * quadDensity.y;
        }

        public void Generate()
        {
            Awake();
            
            Texture2D  stretchedHeightmap = rawHeightmap; // TODO: add support for automatically stretching textures
            int        pixelsWide = stretchedHeightmap.width;
            int        pixelsHeight = stretchedHeightmap.height;
            Vector2Int originPos = new Vector2Int(
                Mathf.RoundToInt(pixelsWide * offset.x), 
                Mathf.RoundToInt(pixelsHeight * offset.y));

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
    }
}

# endif