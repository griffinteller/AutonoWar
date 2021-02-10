using Unity.Mathematics;
using UnityEngine;

namespace GameTerrain
{
    [RequireComponent(typeof(LODGroup))]
    public class TorusQuadRenderer : MonoBehaviour
    {
        public TorusTerrainController parentController;
        public int                    longitude;
        public int                    latitude;
        public byte                   edgeMask;

        [HideInInspector] public LODGroup       lodGroup;
        [HideInInspector] public MeshFilter[]   meshFilters;
        [HideInInspector] public MeshRenderer[] meshRenderers;
        [HideInInspector] public MeshCollider meshCollider;

        public int RendererIndex => longitude * parentController.latitudes + latitude;

        public Vector3 LocalCenter
        {
            get
            {
                float majorAngle = longitude * parentController.majorQuadArc;
                float minorAngleRad = latitude * parentController.minorQuadArc * Mathf.Deg2Rad;
                return Quaternion.Euler(0, -majorAngle, 0)
                       * new Vector3(
                           parentController.majorRadius + parentController.minorRadius * Mathf.Cos(minorAngleRad),
                           parentController.minorRadius * Mathf.Sin(minorAngleRad),
                           0);
            }
        }
        
        public void Awake()
        {
            lodGroup = GetComponent<LODGroup>();
        }

# if UNITY_EDITOR

        public void ApplyMaterial(TorusMaterialInfo info)
        {
            for (int lod = 0; lod < parentController.Lods; lod++)
            {
                int      degree   = parentController.lodQuadDegrees[lod];
                Material material = GetRendererMaterial(info, degree);
                meshRenderers[lod].material = material;
            }
        }

        public void SetCollider(PhysicMaterial material)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilters[0].sharedMesh;
            meshCollider.enabled = false;
        }
        
        private Material GetRendererMaterial(TorusMaterialInfo info, int degree)
        {
            float majorQuadArc = parentController.majorQuadArc;
            float minorQuadArc = parentController.minorQuadArc;
            float majorRadius  = parentController.majorRadius;
            float minorRadius  = parentController.minorRadius;
            
            Material material = new Material(info.Shader);
            material.SetFloat("_mRad", minorRadius);
            material.SetFloat("_MRad", majorRadius);
            material.SetInt("_Longs", parentController.longitudes);
            material.SetInt("_Lats", parentController.latitudes);
            material.SetInt("_Long", longitude);
            material.SetInt("_Lat", latitude);

            float normTileSizeOnOutside = info.TileSize.x / (majorQuadArc * Mathf.Deg2Rad * (majorRadius + minorRadius));
            int   rowsPerQuad           = (int) (minorQuadArc * Mathf.Deg2Rad * minorRadius / info.TileSize.y + 0.5f);

            material.SetInt("_RowsPerQuad", rowsPerQuad);
            material.SetInt("_ColsPerQuad", info.ColsPerQuad);
            material.SetFloat("_NormTileSizeOnOutside", normTileSizeOnOutside);
            material.SetTexture("_Splatmap0", info.Splatmap0);
            material.SetTexture("_Layer0", info.Layer0);
            material.SetTexture("_Layer1", info.Layer1);
            material.SetTexture("_Layer2", info.Layer2);
            material.SetTexture("_Layer3", info.Layer3);

            if (info.UseHeightmapOffset)
                info.Offset = parentController.heightmap.offset;
            
            material.SetVector("_Offset", info.Offset);
            material.SetInt("_LOD", 7 - degree); /* we actually do want to do this, and not find the 
                                                            corresponding lod, since the texture lod should have to do
                                                            with the real density of vertices
                                                            */

            return material;
        }
        
        public void GenerateMeshes()
        {
            Awake();
            
            int    numberOfLods = parentController.Lods;
            LOD[]  lods         = new LOD[numberOfLods];
            meshFilters   = new MeshFilter[numberOfLods];
            meshRenderers = new MeshRenderer[numberOfLods];

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

                lods[lod]         = new LOD(
                    parentController.lodViewPercentages[lod], 
                    new Renderer[] {meshRenderer});
                
                meshFilters[lod]   = meshFilter;
                meshRenderers[lod] = meshRenderer;
            }
            
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            foreach (MeshRenderer renderer in meshRenderers)
                renderer.scaleInLightmap *= parentController.lightmapScale;
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

        public void ApplyNormalArrays(Vector3[][] normalArrays)
        {
            for (int lod = 0; lod < parentController.Lods; lod++)
                meshFilters[lod].sharedMesh.normals = normalArrays[lod];
        }

        public Vector3[][] GetAdjustedNormalArrays()
        {
            int         lods   = parentController.Lods;
            Vector3[][] result = new Vector3[lods][];

            for (int lod = 0; lod < lods; lod++)
                result[lod] = AdjustSeamNormalsAtLOD(lod);

            return result;
        }
        
        private Vector3[] AdjustSeamNormalsAtLOD(int lod)
        {
            // calculates new normals, but doesn't apply them, because other adjacent renderers need to calculate using
            // old ones
            
            int numberOfRenderers = parentController.renderers.Length;
            int longitudes        = parentController.longitudes;
            int latitudes         = parentController.latitudes;
            int degree            = parentController.lodQuadDegrees[lod];
            int verticesPerSide   = (int) (math.pow(2, degree) + 0.5f) + 1;

            Vector3[] topMeshNormals = 
                parentController.renderers[(longitude * latitudes + latitude + 1) % numberOfRenderers]
                                .meshFilters[lod].sharedMesh.normals;
            Vector3[] rightMeshNormals =
                parentController.renderers[((longitude + 1) * latitudes + latitude) % numberOfRenderers]
                                .meshFilters[lod].sharedMesh.normals;
            Vector3[] bottomMeshNormals =
                parentController.renderers[(longitude * latitudes + latitude - 1 + numberOfRenderers) % numberOfRenderers]
                                .meshFilters[lod].sharedMesh.normals;
            Vector3[] leftMeshNormals =
                parentController.renderers[((longitude - 1) * latitudes + latitude + numberOfRenderers) % numberOfRenderers]
                                .meshFilters[lod].sharedMesh.normals;

            Mesh    sharedMesh = meshFilters[lod].sharedMesh;
            Vector3[] normals    = sharedMesh.normals;
            
            // top normals
            int penultimateRow = verticesPerSide * (verticesPerSide - 1);
            for (int vertex = 0; vertex < verticesPerSide; vertex++)
                normals[vertex] += topMeshNormals[penultimateRow + vertex];

            // right normals
            for (int vertex = verticesPerSide - 1; vertex < verticesPerSide * verticesPerSide; vertex += verticesPerSide)
                normals[vertex] += rightMeshNormals[vertex - (verticesPerSide - 1)];
            
            // bottom normals
            for (int vertex = penultimateRow; vertex < verticesPerSide * verticesPerSide; vertex++)
                normals[vertex] += bottomMeshNormals[vertex - penultimateRow];
            
            // left normals 
            for (int vertex = 0; vertex < verticesPerSide * verticesPerSide; vertex += verticesPerSide)
                normals[vertex] += leftMeshNormals[vertex + (verticesPerSide - 1)];
            
            // normalize 
            
            // top normals
            for (int vertex = 0; vertex < verticesPerSide; vertex++)
                normals[vertex] = normals[vertex].normalized;

            // right normals
            for (int vertex = verticesPerSide - 1; vertex < verticesPerSide * verticesPerSide; vertex += verticesPerSide)
                normals[vertex] = normals[vertex].normalized;
            
            // bottom normals
            for (int vertex = penultimateRow; vertex < verticesPerSide * verticesPerSide; vertex++)
                normals[vertex] = normals[vertex].normalized;
            
            // left normals 
            for (int vertex = 0; vertex < verticesPerSide * verticesPerSide; vertex += verticesPerSide)
                normals[vertex] = normals[vertex].normalized;

            return normals;
        }
# endif
    }
}