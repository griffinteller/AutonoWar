using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utility;

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

            return material;
        }

        public override JobHandle ScheduleVerticesJob(int rendererIndex)
        {
            TerrainQuadRenderer renderer = Renderers[rendererIndex];

            if (renderer.Vertices.IsCreated)
                renderer.Vertices.Dispose();

            byte lod = LODs[rendererIndex];
            renderer.Vertices = new NativeArray<Vector3>(VerticesPerDegree[lod], Allocator.TempJob);
            
            GenerateTorusVerticesJob job = new GenerateTorusVerticesJob
            {
                Latitudes             = latitudes,
                RendererIndex         = rendererIndex,
                MajorRadius           = majorRadius,
                MinorRadius           = minorRadius,
                MajorArcRad           = math.radians(majorArc),
                MinorArcRad           = math.radians(minorArc),
                LOD                   = LODs[rendererIndex],
                HeightmapMapMaxDegree = Heightmap.maxDegree,
                Heightmap             = Heightmap.Heights,
                Vertices = Renderers[rendererIndex].Vertices
                
            };

            JobHandle handle = job.Schedule();

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
                    
                    Vertices[vertexIndex] = MathUtil.Float4ToVec3(vertex);
                }
        }
    }

    /*public struct GenerateTorusNormalsJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> Vertices;
        [ReadOnly] public NativeArray<int>
        
        [ReadOnly] public NativeArray<Vector3> TopVertices;
        [ReadOnly] public NativeArray<Vector3> RightVertices;
        [ReadOnly] public NativeArray<Vector3> BottomVertices;
        [ReadOnly] public NativeArray<Vector3> LeftVertices;

        public NativeArray<Vector3> Normals;
    }*/
}