using System;
using UnityEngine;

namespace GameTerrain
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public abstract class TerrainQuadRenderer : MonoBehaviour
    {
        public TerrainQuad Quad { get; private set; }

        public Vector3 LocalCenter
        {
            get => _localCenter;
            private set => _localCenter = value;
        } // center of the quad relative to the center of the terrain
        // should NOT change with degree

        public byte MipmapMask { get; set; }

        protected abstract TerrainQuad GenerateQuad(TriangleCache cache);
        protected abstract Vector3 GetLocalCenter();

        private TriangleCache _triangleCache;
        private MeshFilter _meshFilter;
        [SerializeField] private Vector3 _localCenter;

        public void Start()
        {
            _triangleCache = GetComponentInParent<TerrainController>().TriangleCache;
            _meshFilter = GetComponent<MeshFilter>();

            LocalCenter = GetLocalCenter();
            
            RefreshMesh();
        }

        public void RefreshMesh()
        {
            Quad = GenerateQuad(_triangleCache);
            _meshFilter.mesh = Quad.Mesh;
        }
    }
}