using System;
using GameTerrain.Flat;
using UnityEngine;

namespace GameTerrain
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TerrainQuadRenderer : MonoBehaviour
    {
        public TerrainQuad quad;

        public void Start()
        {
            const int degree = 3;
            const float width = 5;
            
            var triangleCache = GetComponentInParent<TerrainController>().TriangleCache;
            var heightmap = TerrainQuad.GetRandomHeights(degree, (-1, 1));
            quad = new FlatTerrainQuad(width, degree, triangleCache, heightmap);
            GetComponent<MeshFilter>().mesh = quad.Mesh;
        }
    }
}