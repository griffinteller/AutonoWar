using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTerrain
{
    [CreateAssetMenu]
    public class QuadTerrainHeightmap : ScriptableObject
    {
        [HideInInspector] [SerializeField] private int[] heights; // longitude, latitude, row, col
        [SerializeField] private int _longitudinalDensity;
        [SerializeField] private int _latitudinalDensity;
        [SerializeField] private byte _maxDegree;

        private int _verticesPerSide;
        private int _verticesPerQuad;
        private int _verticesPerLongitude;

        public int LongitudinalDensity => _longitudinalDensity;
        public int LatitudinalDensity => _latitudinalDensity;
        public byte MaxDegree => _maxDegree;

        public void Awake()
        {
            _verticesPerSide = (int) (Mathf.Pow(2, _maxDegree) + 0.5f) + 1;
            _verticesPerQuad = _verticesPerSide * _verticesPerSide;
            _verticesPerLongitude = _verticesPerQuad * _latitudinalDensity;
        }

        public int GetVertex(int longitude, int latitude, int row, int col, byte degree)
        {
            int step = (int) (Mathf.Pow(2, _maxDegree - degree) + 0.5f);
            return heights[
                longitude * _verticesPerLongitude 
                + latitude * _verticesPerQuad 
                + row * step * _verticesPerSide 
                + col * step];
        }
        
        # if UNITY_EDITOR
        
        [SerializeField] private Texture2D rawHeightmap;

        public void Generate()
        {
            
        }
        
        # endif
    }
}