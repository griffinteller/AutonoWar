using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using static System.Single;

namespace GameTerrain.Torus
{
    public class TorusTerrainController : QuadTerrainController
    {
        public float majorRadius;
        public float minorRadius;
        public float majorArc; // degrees
        public float minorArc;

        [Header("Shader Settings")] public Vector2   tileSize;
        public                             int       colsPerQuad;
        public                             bool      useHeightmapOffset;
        public                             Vector2   offset;
        public                             Texture2D splatmap0;
        public                             Texture2D layer0;
        public                             Texture2D layer1;
        public                             Texture2D layer2;
        public                             Texture2D layer3;

        public override void OnEnable()
        {
            base.OnEnable();
            
            majorArc = 360f / longitudes;
            minorArc = 360f / latitudes;
        }

        public override TerrainQuadRenderer[] GenerateRenderers()
        {
            TerrainQuadRenderer[] terrainQuadRenderers = new TerrainQuadRenderer[longitudes * latitudes];
            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                TerrainQuadRenderer renderer = new GameObject(
                    "Renderer " + longitude + ":" + latitude,
                    typeof(TerrainQuadRenderer))
                   .GetComponent<TerrainQuadRenderer>();

                renderer.transform.parent = transform;

                

                renderer.longitude   = longitude;
                renderer.latitude    = latitude;

                float minorAngleRad = math.radians(minorArc * latitude);
                float majorAngleRad = math.radians(majorArc * longitude);
                renderer.localCenter = math.mul(
                    float4x4.RotateY(-majorAngleRad),
                    new float4(
                        majorRadius + minorRadius * math.cos(minorAngleRad), 
                        minorRadius * math.sin(minorAngleRad), 
                        0, 
                        0));

                renderer.TriangleCache = TriangleCache;
                renderer.UvCache       = UvCache;

                renderer.calculateNormals = false;

                renderer.GetComponent<MeshRenderer>().material = GetRendererMaterial(renderer);
                
                terrainQuadRenderers[longitude * latitudes + latitude] = renderer;
            }

            return terrainQuadRenderers;
        }

        /*
        * Should be called as the very last step of renderer generation
        */
        private Material GetRendererMaterial(TerrainQuadRenderer renderer)
        {
            Material material = new Material(shader);
            material.SetFloat("_mRad", minorRadius);
            material.SetFloat("_MRad", majorRadius);
            material.SetInt("_Longs", longitudes);
            material.SetInt("_Lats", latitudes);
            material.SetInt("_Long", renderer.longitude);
            material.SetInt("_Lat", renderer.latitude);

            float normTileSizeOnOutside = tileSize.x / (majorArc * Mathf.Deg2Rad * (majorRadius + minorRadius));
            int   rowsPerQuad           = (int) (minorArc * Mathf.Deg2Rad * minorRadius / tileSize.y + 0.5f);

            material.SetInt("_RowsPerQuad", rowsPerQuad);
            material.SetInt("_ColsPerQuad", colsPerQuad);
            material.SetFloat("_NormTileSizeOnOutside", normTileSizeOnOutside);
            material.SetTexture("_Splatmap0", splatmap0);
            material.SetTexture("_Layer0", layer0);
            material.SetTexture("_Layer1", layer1);
            material.SetTexture("_Layer2", layer2);
            material.SetTexture("_Layer3", layer3);

            if (useHeightmapOffset)
                offset = Heightmap.offset;
            
            material.SetVector("_Offset", offset);

            return material;
        }

        public override JobHandle ScheduleVerticesJob(int rendererIndex)
        {
            TerrainQuadRenderer renderer = Renderers[rendererIndex];

            renderer.Vertices.TryDispose();
            renderer.Normals.TryDispose();

            byte lod      = LODs[rendererIndex];
            byte edgeMask = EdgeMasks[rendererIndex];
            renderer.Vertices = new NativeArray<Vector3>(VerticesPerDegree[lod], Allocator.TempJob);
            renderer.Normals  = new NativeArray<Vector3>(VerticesPerDegree[lod], Allocator.TempJob);
            
            GenerateTorusVerticesJob vertexJob = new GenerateTorusVerticesJob
            {
                Latitudes             = latitudes,
                RendererIndex         = rendererIndex,
                MajorRadius           = majorRadius,
                MinorRadius           = minorRadius,
                MajorArcRad           = math.radians(majorArc),
                MinorArcRad           = math.radians(minorArc),
                LOD                   = lod,
                HeightmapMapMaxDegree = Heightmap.maxDegree,
                Heightmap             = Heightmap.Heights,
                Vertices              = renderer.Vertices
                
            };

            GenerateTorusNormalsJob normalsJob = new GenerateTorusNormalsJob
            {
                MajorArcRad = math.radians(majorArc),
                MinorArcRad = math.radians(minorArc),
                Long = renderer.longitude,
                Lat = renderer.latitude,
                EdgesPerSide = (int) (math.pow(2, lod) + 0.5f),
                Vertices  = renderer.Vertices,
                Triangles = TriangleCache.Cache[lod][edgeMask],
                Normals   = renderer.Normals
            };
            

            JobHandle vertexHandle = vertexJob.Schedule();
            JobHandle handle       = normalsJob.Schedule(vertexHandle);

            return handle;
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct GenerateTorusVerticesJob : IJob
    {
        public int Latitudes;
        public int RendererIndex;

        public float MajorRadius;
        public float MinorRadius;
        public float MajorArcRad;
        public float MinorArcRad;

        public byte LOD;
        public byte HeightmapMapMaxDegree;
        
        [ReadOnly] public NativeArray<float> Heightmap;

        [WriteOnly] public NativeArray<Vector3> Vertices;

        public void Execute()
        {
            int longitude                = RendererIndex / Latitudes;
            int latitude                 = RendererIndex % Latitudes;
            int heightmapVerticesPerSide = (int) (math.pow(2, HeightmapMapMaxDegree) + 0.5f) + 1;
            int heightmapStartIndex = heightmapVerticesPerSide * heightmapVerticesPerSide * RendererIndex;
            int heightmapStep = (int) (math.pow(2, HeightmapMapMaxDegree - LOD) + 0.5f); // if we are at a low
                                                                                             // lod, then we should skip
                                                                                             // some heightmap info
            
            int verticesPerSide    = (int) (math.pow(2, LOD) + 0.5f) + 1;

            float leftEdgeMajorAngle = MajorArcRad * (longitude       - 0.5f);
            float topEdgeMinorAngle  = MinorArcRad * (latitude        + 0.5f);
            float majorVertexStep    = MajorArcRad / (verticesPerSide - 1);
            float minorVertexStep    = MinorArcRad / (verticesPerSide - 1);

            for (int row = 0; row < verticesPerSide; row++)
                for (int col = 0; col < verticesPerSide; col++)
                {
                    float    minorAngle = topEdgeMinorAngle - row * minorVertexStep;
                    float4x4 majorRot   = float4x4.RotateY(-(leftEdgeMajorAngle + col * majorVertexStep));

                    float4 basePos = math.mul(
                        majorRot,
                        new float4(
                            MajorRadius + MinorRadius * math.cos(minorAngle), 
                            MinorRadius * math.sin(minorAngle), 
                            0, 
                            0));

                    float4 normal = math.mul(
                        majorRot, 
                        new float4(math.cos(minorAngle), math.sin(minorAngle), 0, 0));

                    int    vertexIndex = row * verticesPerSide + col;
                    float4 vertex = basePos + normal * Heightmap[
                        heightmapStartIndex 
                      + row * heightmapStep * heightmapVerticesPerSide 
                      + col * heightmapStep];
                    
                    Vertices[vertexIndex] = vertex.xyz;
                }
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct GenerateTorusNormalsJob : IJob
    {
        public float MajorArcRad;
        public float MinorArcRad;
        public int Long;
        public int Lat;
        public int EdgesPerSide;
        
        [ReadOnly] public NativeArray<Vector3> Vertices;
        [ReadOnly] public NativeArray<int>     Triangles;
        
        /*[ReadOnly] public NativeArray<Vector3> TopVertices;
        [ReadOnly] public NativeArray<Vector3> RightVertices;
        [ReadOnly] public NativeArray<Vector3> BottomVertices;
        [ReadOnly] public NativeArray<Vector3> LeftVertices;
        
        [ReadOnly] public NativeArray<int> TopTriangles;
        [ReadOnly] public NativeArray<int> RightTriangles;
        [ReadOnly] public NativeArray<int> BottomTriangles;
        [ReadOnly] public NativeArray<int> LeftTriangles;*/

        public NativeArray<Vector3> Normals;

        public void Execute()
        {
            NativeArray<float4> verticesFlt4     = new NativeArray<float4>(Vertices.Length, Allocator.Temp);
            NativeArray<float4> normalsFlt4      = new NativeArray<float4>(Normals.Length, Allocator.Temp);
            NativeArray<bool>   accessedVertices = new NativeArray<bool>(Vertices.Length, Allocator.Temp);
            
            verticesFlt4.FromVector3s(Vertices);

            for (int vertex = 0; vertex < Triangles.Length; vertex += 3)
            {
                int     v0Index = Triangles[vertex];
                int     v1Index = Triangles[vertex + 1];
                int     v2Index = Triangles[vertex + 2];
                
                float4 normal  = GetFaceNormal(
                    verticesFlt4[v0Index], verticesFlt4[v1Index], verticesFlt4[v2Index]);

                normalsFlt4[v0Index] += normal;
                normalsFlt4[v1Index] += normal;
                normalsFlt4[v2Index] += normal;

                accessedVertices[v0Index] = true;
                accessedVertices[v1Index] = true;
                accessedVertices[v2Index] = true;
            }

            for (int normal = 0; normal < Normals.Length; normal++)
            {
                float4  normFlt4 = normalsFlt4[normal];
                if (!accessedVertices[normal])
                    Normals[normal] = Vector3.zero;
                else
                {
                    Vector3 normVec3 = math.normalize(normFlt4).xyz;
                    Normals[normal] = normVec3;
                }
            }

            /*float vertAngleStep  = MinorArcRad / EdgesPerSide;
            float horizAngleStep = MajorArcRad / EdgesPerSide;

            float leftAngle   = MajorArcRad * (Long - 0.5f);
            float rightAngle  = MajorArcRad * (Long + 0.5f);
            float topAngle    = MinorArcRad * (Lat  + 0.5f);
            float bottomAngle = MinorArcRad * (Lat  - 0.5f);

            float4 topUnrotVector = new float4(
                math.cos(topAngle),
                math.sin(topAngle),
                0,
                0);

            float4 bottomUnrotVector = new float4(
                math.cos(bottomAngle),
                math.sin(bottomAngle),
                0,
                0);

            float4x4 majorRot;
            
            // top edge
            for (int vertex = 0; vertex < EdgesPerSide + 1; vertex++)
            {
                majorRot = float4x4.RotateY(-(leftAngle + vertex * horizAngleStep));
                Normals[vertex] = math.normalize(math.mul(majorRot, topUnrotVector)).xyz;
            }

            // bottom edge
            int startIndex = Normals.Length - EdgesPerSide - 1;
            for (int vertex = 0; vertex < EdgesPerSide + 1; vertex++)
            {
                majorRot = float4x4.RotateY(-(leftAngle + vertex * horizAngleStep));
                Normals[startIndex + vertex] = math.normalize(math.mul(majorRot, bottomUnrotVector)).xyz;
            }
            
            // left edge
            majorRot = float4x4.RotateY(-leftAngle);
            for (int vertex = 0; vertex < EdgesPerSide + 1; vertex++)
            {
                float    vertAngle = topAngle - vertex * vertAngleStep;
                Normals[vertex * (EdgesPerSide + 1)] = math.normalize(math.mul(
                    majorRot, 
                    new float4(
                        math.cos(vertAngle),
                        math.sin(vertAngle),
                        0,
                        0))).xyz;
            }

            // right edge
            majorRot   = float4x4.RotateY(-rightAngle);
            startIndex = EdgesPerSide;
            for (int vertex = 0; vertex < EdgesPerSide + 1; vertex++)
            {
                float vertAngle = topAngle - vertex * vertAngleStep;
                Normals[startIndex + vertex * (EdgesPerSide + 1)] = math.normalize(math.mul(
                    majorRot, 
                    new float4(
                        math.cos(vertAngle),
                        math.sin(vertAngle),
                        0,
                        0))).xyz;
            }*/

            verticesFlt4.Dispose();
            normalsFlt4.Dispose();
            accessedVertices.Dispose();
        }

        public static float4 GetFaceNormal(float4 p1, float4 p2, float4 p3)
        {
            return math.normalize(new float4(math.cross((p2 - p1).xyz, (p3 - p1).xyz), 0));
        }
    }
}