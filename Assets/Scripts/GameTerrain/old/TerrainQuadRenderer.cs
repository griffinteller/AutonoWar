using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace GameTerrain.old
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TerrainQuadRenderer : MonoBehaviour
    {

        public                 NativeArray<Vector3> Vertices;
        public                 NativeArray<Vector3> Normals;
        public                 byte                 lod = 1;
        public                 byte                 edgeMask;
        public                 bool                 calculateNormals = true;
        [NonSerialized] public TriangleCache        TriangleCache;
        [NonSerialized] public UvCache              UvCache;

        public float4                                localCenter;
        public int                                   longitude;
        public int                                   latitude;

        private MeshFilter _meshFilter;

        public void Awake()
        {
            _meshFilter      = GetComponent<MeshFilter>();
            _meshFilter.mesh = new Mesh();
        }

        public void FullyRefreshMesh()
        {
            /*
             * This should only be done after doing a vertex job, and should be done directly after completion,
             * since the Vertices array is allocated as TempJob
             */
            
            Mesh mesh = new Mesh();
            
            mesh.Clear();
            mesh.vertices = Vertices.ToArray();
            mesh.triangles = TriangleCache.Cache[lod][edgeMask].ToArray();
            mesh.uv = UvCache.Cache[lod];

            if (calculateNormals)
                mesh.RecalculateNormals();
            else
                mesh.normals = Normals.ToArray();

            _meshFilter.mesh = mesh;

            Vertices.TryDispose();
            Normals.TryDispose();
        }

        public void RefreshTriangles()
        {
            /*
             * This should be run when the edge mask has changed, but not the lod
             */
            
            Mesh    mesh  = _meshFilter.sharedMesh;
            Vector3[] verts = mesh.vertices;

            Vector3[] normals = mesh.normals;
            
            mesh.Clear();
            mesh.vertices  = verts;
            mesh.triangles = TriangleCache.Cache[lod][edgeMask].ToArray();
            mesh.uv        = UvCache.Cache[lod];
            mesh.normals   = normals;
        }

        public void DisposeNativeArrays()
        {
            Vertices.TryDispose();
            Normals.TryDispose();
        }
    }
}