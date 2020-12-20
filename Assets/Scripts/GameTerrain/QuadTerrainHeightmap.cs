using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        
        [HideInInspector] public float[] heightsStored; // longitude, latitude, row, col
        
        [SerializeField] private Vector2Int quadDensity;
        [SerializeField] private byte maxDegree;

        [NonSerialized] public NativeArray<float> Heights;

        private int _verticesPerSide;
        private int _verticesPerQuad;
        private int _verticesPerLongitude;

        public Vector2Int QuadDensity => quadDensity;
        public byte MaxDegree => maxDegree;

        public void OnEnable()
        {
            # if UNITY_EDITOR

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("Not playing! Cancelling heightmap load...");

                return;
            }
            
            # endif
            
            Heights = new NativeArray<float>(heightsStored, Allocator.Persistent);
            
            // TODO: somehow dereference stored heightmap without saving deletion to disk
            // heightsStored = null; // dereferences old array so it will be gc-ed
        }

        public void OnDisable()
        {
            if (Heights.IsCreated)
                Heights.Dispose();
        }

        public void OnDestroy()
        {
            OnDisable();
        }

        # if UNITY_EDITOR
        
        public void Awake()
        {
            _verticesPerSide = (int) (Mathf.Pow(2, maxDegree) + 0.5f) + 1;
            _verticesPerQuad = _verticesPerSide * _verticesPerSide;
            _verticesPerLongitude = _verticesPerQuad * quadDensity.y;
        }
        
        [SerializeField] private Texture2D rawHeightmap;
        [SerializeField] private Vector2Int originPos;
        [SerializeField] private float maxPossibleHeight;

        public void Generate()
        {
            Awake();
            
            Texture2D stretchedHeightmap = rawHeightmap; // TODO: add support for automatically stretching textures
            int pixelsWide = stretchedHeightmap.width;
            int pixelsHeight = stretchedHeightmap.height;
            
            heightsStored = new float[quadDensity.x * _verticesPerLongitude];
            
            for (int longitude = 0; longitude < quadDensity.x; longitude++)
            for (int latitude = 0; latitude < quadDensity.y; latitude++)
            for (int row = 0; row < _verticesPerSide; row++)
            for (int col = 0; col < _verticesPerSide; col++)
            {
                int pixelX = (longitude * _verticesPerSide + col - _verticesPerSide / 2 + originPos.x +
                              pixelsWide) % pixelsWide;
                int pixelY = (latitude * _verticesPerSide - row - _verticesPerSide / 2 + originPos.y +
                              pixelsHeight) % pixelsHeight;
                heightsStored[longitude * _verticesPerLongitude
                        + latitude * _verticesPerQuad
                        + row * _verticesPerSide
                        + col] = stretchedHeightmap.GetPixel(pixelX, pixelY).r * maxPossibleHeight;
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        # endif
    }
}