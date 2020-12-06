using System;
using UnityEngine;

namespace GameTerrain
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public abstract class TerrainQuadRenderer : MonoBehaviour
    {
        public TerrainQuad Quad { get; private set; }

        protected abstract TerrainQuad GenerateQuad(TriangleCache cache);
        
        public void Start()
        {
            TriangleCache triangleCache = GetComponentInParent<TerrainController>().TriangleCache;
            Quad = GenerateQuad(triangleCache);
            GetComponent<MeshFilter>().mesh = Quad.Mesh;
        }
    }
}