using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utility;

namespace GameTerrain
{
    public abstract class QuadTerrainController : MonoBehaviour
    {
        /*
         * Assumes Grid layout of quads. Might make more abstract class later if this is unacceptable for a sphere or
         * whatever
         */
        
        public static readonly int[] VerticesPerDegree = {4, 9, 25, 81, 289, 1089, 4225, 16641};

        public Shader shader;
        
        [SerializeField] protected int            longitudes;
        [SerializeField] protected int            latitudes;
        
        [SerializeField] private Transform      lodReference;
        [SerializeField] private AssetReference triangleCacheReference;
        [SerializeField] private AssetReference heightmapReference;
        [SerializeField] private AssetReference uvCacheReference;
        [SerializeField] private float[]        lodDistancesSerialized;
        [SerializeField] private byte[]         lodsByDistanceSerialized;

        public NativeArray<float4> LocalCenters;

        protected TriangleCache         TriangleCache;
        protected QuadTerrainHeightmap  Heightmap;
        protected UvCache               UvCache;
        protected NativeArray<byte>     LODs;
        public   NativeArray<byte>     EdgeMasks;
        protected TerrainQuadRenderer[] Renderers;

        private AsyncOperationHandle<TriangleCache>        _triangleCacheHandle;
        private AsyncOperationHandle<QuadTerrainHeightmap> _heightmapHandle;
        private AsyncOperationHandle<UvCache>              _uvCacheHandle;
        private NativeArray<byte>                          _dirtyRenderers; // 00 - not dirty; 01 - just triangles;
                                                                            // >=10 - vertices and tris
        private NativeArray<float> _lodDistances;
        private NativeArray<byte>  _lodsByDistance;
        private JobHandle          _verticesJobHandle;
        private JobHandle          _researchJobHandle;
        private bool               _justScheduledResearchJobs; // lod and edge mask finding jobs
                                                               // , and other ones like that
        private bool ResearchJobsDone => _researchJobHandle.IsCompleted;
        private bool VertexJobsDone   => _verticesJobHandle.IsCompleted;

        public bool Loaded    { get; private set; }
        public int  NumberOfRenderers { get; private set; }
            
        public abstract TerrainQuadRenderer[] GenerateRenderers();
        public abstract JobHandle             ScheduleVerticesJob(int rendererIndex);

        public virtual void OnEnable()
        {
            _triangleCacheHandle = Addressables.LoadAssetAsync<TriangleCache>(triangleCacheReference);
            _heightmapHandle     = Addressables.LoadAssetAsync<QuadTerrainHeightmap>(heightmapReference);
            _uvCacheHandle       = Addressables.LoadAssetAsync<UvCache>(uvCacheReference);
        }

        public void OnDisable()
        {
            if (!Loaded)
                return;
            
            JobHandle.CompleteAll(ref _researchJobHandle, ref _verticesJobHandle);

            DisposeNativeArrays();
            
            foreach (TerrainQuadRenderer renderer in Renderers)
                renderer.DisposeNativeArrays();
            
            Heightmap.DisposeNativeArrays();
            TriangleCache.DisposeNativeArrays();

            Addressables.Release(_triangleCacheHandle);
            Addressables.Release(_heightmapHandle);
            Addressables.Release(_uvCacheHandle);
            
            Loaded = false;
        }

        public void DisposeNativeArrays()
        {
            LODs.TryDispose(); 
            LocalCenters.TryDispose();
            EdgeMasks.TryDispose();
            _dirtyRenderers.TryDispose();
            _lodDistances.TryDispose();
            _lodsByDistance.TryDispose();
        }

        public void OnApplicationQuit()
        {
            OnDisable();
        }

        public void Update()
        {
            if (!Loaded)
            {
                Loaded = _triangleCacheHandle.IsDone && _heightmapHandle.IsDone && _uvCacheHandle.IsDone;

                if (!Loaded)
                    return;

                TriangleCache = _triangleCacheHandle.Result;
                Heightmap = _heightmapHandle.Result;
                UvCache = _uvCacheHandle.Result;
                
                Renderers = GenerateRenderers();
                NumberOfRenderers  = Renderers.Length;
                
                InitializeNativeArrays();
                UpdateLocalCenters();
                ScheduleResearchJobs();
                _justScheduledResearchJobs = true;

                return;
            }

            if (!_justScheduledResearchJobs && VertexJobsDone)
            {
                _verticesJobHandle.Complete();
                
                RefreshDirtyRenderers();
                
                ScheduleResearchJobs();
                _justScheduledResearchJobs = true;
            }
            else if (_justScheduledResearchJobs && ResearchJobsDone)
            {
                _researchJobHandle.Complete();
                ScheduleVerticesJobs();
                _verticesJobHandle         = JobHandle.CombineDependencies(
                    _verticesJobHandle,
                    SchedulePostVerticesJobs());
                _justScheduledResearchJobs = false;
            }
        }

        private void ScheduleVerticesJobs()
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                if (_dirtyRenderers[i] >= 2) // if lod has changed, not just triangles;
                {
                    JobHandle handle = ScheduleVerticesJob(i);
                    _verticesJobHandle = JobHandle.CombineDependencies(_verticesJobHandle, handle);
                }
            }
        }

        public virtual JobHandle SchedulePostVerticesJobs()
        {
            return new JobHandle();
        }

        private void InitializeNativeArrays()
        {
            int renderers = NumberOfRenderers;
            
            EdgeMasks      = new NativeArray<byte>(renderers, Allocator.Persistent);
            _lodDistances   = new NativeArray<float>(lodDistancesSerialized, Allocator.Persistent);
            _lodsByDistance = new NativeArray<byte>(lodsByDistanceSerialized, Allocator.Persistent);
            
            LODs         = new NativeArray<byte>(renderers, Allocator.Persistent);
            LocalCenters = new NativeArray<float4>(renderers, Allocator.Persistent);
        }

        private void ScheduleResearchJobs()
        {
            float4 referencePos = (Vector4) transform.InverseTransformPoint(lodReference.position);

            _dirtyRenderers = new NativeArray<byte>(NumberOfRenderers, Allocator.TempJob);

            GetLODsJob lodsJob = new GetLODsJob
            {
                DirtyRenderers = _dirtyRenderers,
                LocalCamPos    = referencePos,
                LocalCenters   = LocalCenters,
                LODDistances   = _lodDistances,
                LODsByDistance = _lodsByDistance,
                RendererLODs   = LODs,
            };

            GetWrappedEdgeMasksJob edgeMasksJob = new GetWrappedEdgeMasksJob
            {
                DirtyRenderers = _dirtyRenderers,
                EdgeMasks      = EdgeMasks,
                LODs           = LODs,
                Latitudes      = latitudes
            };
            
            JobHandle lodJobHandle              = lodsJob.Schedule(NumberOfRenderers, 16);
            JobHandle edgeMaskJobHandle         = edgeMasksJob.Schedule(
                NumberOfRenderers, 16, lodJobHandle);

            _researchJobHandle = JobHandle.CombineDependencies(lodJobHandle, edgeMaskJobHandle);
        }

        private void RefreshDirtyRenderers()
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                TerrainQuadRenderer renderer = Renderers[i];
                renderer.lod      = LODs[i];
                renderer.edgeMask = EdgeMasks[i];

                switch (_dirtyRenderers[i])
                {
                    case 0:
                        break;
                    
                    case 1:
                        renderer.RefreshTriangles();

                        break;
                    
                    default:
                        renderer.FullyRefreshMesh();

                        break;
                }
            }
            
            _dirtyRenderers.Dispose();
        }

        private void UpdateLocalCenters()
        {
            for (int i = 0; i < NumberOfRenderers; i++)
            {
                LocalCenters[i] = Renderers[i].localCenter;
            }
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct GetLODsJob : IJobParallelFor
    {
        public float4 LocalCamPos;
        
        [ReadOnly] public NativeArray<float4> LocalCenters;
        [ReadOnly] public NativeArray<float>  LODDistances;
        [ReadOnly] public NativeArray<byte>   LODsByDistance;
        
        public NativeArray<byte> RendererLODs;
        
        [WriteOnly] public NativeArray<byte> DirtyRenderers;

        public void Execute(int renderer)
        {
            float distance = math.distance(LocalCenters[renderer], LocalCamPos);
            byte  lod = 0;

            for (int i = LODDistances.Length - 1; i >= 0; i--)
            {
                if (distance >= LODDistances[i])
                {
                    lod = LODsByDistance[i];

                    break;
                }
            }

            if (lod != RendererLODs[renderer])
            {
                RendererLODs[renderer]   = lod;
                DirtyRenderers[renderer] = 2;
            }
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct GetWrappedEdgeMasksJob : IJobParallelFor
    {
        public int               Latitudes;
        
        [ReadOnly] public NativeArray<byte> LODs;

        public NativeArray<byte> EdgeMasks;

        public NativeArray<byte> DirtyRenderers;

        public void Execute(int renderer)
        {
            int renderers = EdgeMasks.Length;
            
            int topIndex    = renderer + 1;
            int rightIndex  = renderer + Latitudes;
            int bottomIndex = renderer - 1;
            int leftIndex   = renderer - Latitudes;

            if (topIndex >= renderers)
                topIndex -= renderers;

            if (rightIndex >= renderers)
                rightIndex -= renderers;

            if (bottomIndex < 0)
                bottomIndex += renderers;

            if (leftIndex < 0)
                leftIndex += renderers;

            byte lod  = LODs[renderer];
            byte mask = 0;

            mask |= (byte) (LODs[topIndex] < lod ? 1 : 0);
            mask |= (byte) (LODs[rightIndex] < lod ? 2 : 0);
            mask |= (byte) (LODs[bottomIndex] < lod ? 4 : 0);
            mask |= (byte) (LODs[leftIndex] < lod ? 8 : 0);

            if (mask != EdgeMasks[renderer])
            {
                EdgeMasks[renderer]      = mask;
                DirtyRenderers[renderer] += 1;
            }
        }
    }
}