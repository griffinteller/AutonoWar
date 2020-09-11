using System;
using UnityEngine;

namespace GameTerrain
{
    public abstract class TerrainQuadRenderer : MonoBehaviour
    {
        public TerrainQuad Quad { get; private set; }

        protected abstract TerrainQuad GenerateQuad(TriangleCache cache);
        
        public void Start()
        {
            var triangleCache = GetComponentInParent<TerrainController>().TriangleCache;
            Quad = GenerateQuad(triangleCache);
            GetComponent<MeshFilter>().mesh = Quad.Mesh;
        }
    }
}