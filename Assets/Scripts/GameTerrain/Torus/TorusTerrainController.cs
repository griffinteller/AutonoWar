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
        public float majorArc;
        public float minorArc;
        
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

                renderer.GetComponent<MeshRenderer>().material = material;

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

                terrainQuadRenderers[longitude * latitudes + latitude] = renderer;
            }

            return terrainQuadRenderers;
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
                HeightmapMapMaxDegree = Heightmap.MaxDegree,
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
            int longitude = RendererIndex / Latitudes;
            int latitude  = RendererIndex % Latitudes;
            int heightmapStartIndex = (int) math.pow(math.pow(2, HeightmapMapMaxDegree) + 1, 2)
                                    * RendererIndex;
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
                        MinorRadius * new float4(math.cos(minorAngle), math.sin(minorAngle), 0, 0));

                    int    vertexIndex = row * verticesPerSide + col;
                    float4 vertex = basePos + normal * Heightmap[heightmapStartIndex + vertexIndex * heightmapStep];
                    
                    Vertices[vertexIndex] = MathUtil.Float4ToVec3(vertex);
                }
        }
    }
}