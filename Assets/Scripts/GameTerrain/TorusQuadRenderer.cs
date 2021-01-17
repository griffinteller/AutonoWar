using System;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utility;

namespace GameTerrain
{
    [RequireComponent(typeof(LODGroup))]
    public class TorusQuadRenderer : MonoBehaviour
    {
        public TorusTerrainController parentController;
        public int                    longitude;
        public int                    latitude;
        public byte                   edgeMask;

        private LODGroup _lodGroup;

        public int RendererIndex => longitude * parentController.latitudes + latitude;

        public void Awake()
        {
            _lodGroup = GetComponent<LODGroup>();
        }

# if UNITY_EDITOR
        
        public void GenerateMeshes()
        {
            Awake();
            
            int    numberOfLods = parentController.Lods;
            LOD[]  lods         = new LOD[numberOfLods];

            for (int lod = 0; lod < numberOfLods; lod++)
            {
                int degree = parentController.lodQuadDegrees[lod];
                
                GameObject   obj          = new GameObject("Mesh" + lod);
                Transform    objTransform = obj.transform;
                MeshFilter   meshFilter   = obj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

                obj.isStatic = true;

                objTransform.SetParent(transform);
                objTransform.localPosition = Vector3.zero;

                Mesh mesh = new Mesh();
                mesh.vertices  = GenerateVertices(degree);
                mesh.triangles = parentController.Triangles[degree];
                mesh.uv        = parentController.Uvs[degree];
                mesh.normals = GenerateNormals(degree, mesh.vertices, mesh.triangles);

                meshFilter.mesh       = mesh;
                meshRenderer.material = parentController.material;

                lods[lod] = new LOD(parentController.lodViewPercentages[lod], new Renderer[] {meshRenderer});
            }
            
            _lodGroup.SetLODs(lods);
            _lodGroup.RecalculateBounds();

            foreach (Transform child in transform)
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                renderer.scaleInLightmap *= parentController.lightmapScale;
            }
        }
        
        private Vector3[] GenerateVertices(int degree)
        {
            float majorArcRad = math.radians(parentController.majorQuadArc);
            float minorArcRad = math.radians(parentController.minorQuadArc);
            int   heightmapMapMaxDegree = parentController.heightmap.maxDegree;
            int   heightmapVerticesPerSide = (int) (math.pow(2, heightmapMapMaxDegree) + 0.5f) + 1;
            int   heightmapStartIndex = heightmapVerticesPerSide * heightmapVerticesPerSide * RendererIndex;
            int   heightmapStep = (int) (math.pow(2, heightmapMapMaxDegree - degree) + 0.5f); // if we are at a low
                                                                                             // degree, then we should
                                                                                             // skip some heightmap info
            
            int verticesPerSide    = (int) (math.pow(2, degree) + 0.5f) + 1;

            float leftEdgeMajorAngle = majorArcRad * (longitude       - 0.5f);
            float topEdgeMinorAngle  = minorArcRad * (latitude        + 0.5f);
            float majorVertexStep    = majorArcRad / (verticesPerSide - 1);
            float minorVertexStep    = minorArcRad / (verticesPerSide - 1);

            Vector3[] vertices = new Vector3[verticesPerSide * verticesPerSide];

            for (int row = 0; row < verticesPerSide; row++)
                for (int col = 0; col < verticesPerSide; col++)
                {
                    float    minorAngle = topEdgeMinorAngle - row * minorVertexStep;
                    float4x4 majorRot   = float4x4.RotateY(-(leftEdgeMajorAngle + col * majorVertexStep));

                    float4 basePos = math.mul(
                        majorRot,
                        new float4(
                            parentController.majorRadius + parentController.minorRadius * math.cos(minorAngle), 
                            parentController.minorRadius * math.sin(minorAngle), 
                            0, 
                            0));

                    float4 normal = math.mul(
                        majorRot, 
                        new float4(math.cos(minorAngle), math.sin(minorAngle), 0, 0));

                    int    vertexIndex = row * verticesPerSide + col;
                    float4 vertex = basePos + normal * parentController.Heights[
                        heightmapStartIndex 
                      + row * heightmapStep * heightmapVerticesPerSide 
                      + col * heightmapStep];
                    
                    vertices[vertexIndex] = vertex.xyz;
                }

            return vertices;
        }

        private Vector3[] GenerateNormals(int degree, Vector3[] vertices, int[] triangles)
        {
            // assume adjacent quads are of the same degree

            Vector3[] normals = new Vector3[vertices.Length];
            
            for (int vertex = 0; vertex < triangles.Length; vertex += 3)
            {
                int v0Index = triangles[vertex];
                int v1Index = triangles[vertex + 1];
                int v2Index = triangles[vertex + 2];
                
                Vector3 normal = GetFaceNormal(
                    vertices[v0Index], vertices[v1Index], vertices[v2Index]);

                normals[v0Index] += normal;
                normals[v1Index] += normal;
                normals[v2Index] += normal;
            }

            for (int normal = 0; normal < vertices.Length; normal++)
            {
                normals[normal] = normals[normal].normalized;
            }

            return normals;
        }

        private static Vector3 GetFaceNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Cross(v1 - v0, v2 - v0);
        }

# endif
    }
}