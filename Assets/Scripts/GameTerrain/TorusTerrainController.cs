using System;
using UnityEngine;
using Object = UnityEngine.Object;
# if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

# endif

namespace GameTerrain
{
    public class TorusTerrainController : MonoBehaviour
    {
        public int      majorRadius;
        public int      minorRadius;
        public int      longitudes;
        public int      latitudes;
        public float[]  lodViewPercentages = {0.5f, 0.25f, 0.125f, 0.0625f, 0f};
        public int[]    lodQuadDegrees = {7, 6, 5, 4, 3};
        
        [Header("Collider Settings")] 
        
        public float colliderEnableDistance = 50f;
        public Transform colliderReference;

        [Header("Info, do not edit from editor")] 
        
        public float majorQuadArc; // degrees
        public float minorQuadArc;

        [HideInInspector] public TorusQuadRenderer[] renderers;

        public int Lods => lodViewPercentages.Length;
        
        public void Awake()
        {
            majorQuadArc = 360f / longitudes;
            minorQuadArc = 360f / latitudes;
        }

# if UNITY_EDITOR

        [Header("Generation Only")] 
        
        public QuadTerrainHeightmap heightmap;
        public QuadMeshCache quadMeshCache;
        public float         lightmapScale = 1f;

        [Header("Shader Settings")] public Shader    shader;
        public                             Vector2   tileSize;
        public                             int       colsPerQuad;
        public                             bool      useHeightmapOffset;
        public                             Vector2   offset;
        public                             Texture2D splatmap0;
        public                             Texture2D layer0;
        public                             Texture2D layer1;
        public                             Texture2D layer2;
        public                             Texture2D layer3;

        [Header("Physics Settings")] public PhysicMaterial physicMaterial;

        [NonSerialized] public float[]     Heights;
        [NonSerialized] public int[][]     Triangles;
        [NonSerialized] public Vector2[][] Uvs;

        private void ApplyMaterials()
        {
            TorusMaterialInfo info = new TorusMaterialInfo
            {
                Shader             = shader,
                TileSize           = tileSize,
                ColsPerQuad        = colsPerQuad,
                UseHeightmapOffset = useHeightmapOffset,
                Offset             = offset,
                Splatmap0          = splatmap0,
                Layer0             = layer0,
                Layer1             = layer1,
                Layer2             = layer2,
                Layer3             = layer3
            };
            
            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                int index = longitude * latitudes + latitude;
                renderers[index].ApplyMaterial(info);
            }
        }

        public void GenerateMaterials()
        {
            PrefabStage prefabStage  = PrefabStageUtility.GetCurrentPrefabStage();
            Object      prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            string prefabPath = prefabStage != null ?
                prefabStage.assetPath :
                AssetDatabase.GetAssetPath(prefabSource);

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
                
            foreach (Object asset in allAssets)
                if (asset is Material)
                    DestroyImmediate(asset, true);
            
            ApplyMaterials();
            SaveMaterials();
        }

        public void Update()
        {
            UpdateCollidersEnabled();
        }

        private void UpdateCollidersEnabled()
        {
            // TODO: consider making this asynchronus? depends on performance

            for (int i = 0; i < renderers.Length; i++)
            {
                TorusQuadRenderer renderer = renderers[i];
                Vector3 center = renderer.LocalCenter;
                if (Vector3.Distance(
                    colliderReference.position,
                    transform.TransformPoint(center)) < colliderEnableDistance)
                {
                    renderer.meshCollider.enabled = true;
                    Debug.Log("Enabling a collider!");
                }
                else
                {
                    renderer.meshCollider.enabled = false;
                }
            }
        }

        public void Generate()
        {
            Awake();

            Heights = heightmap.LoadHeightmap();
            QuadMeshCacheData data = quadMeshCache.Deserialize();
            Triangles = data.Triangles;
            Uvs       = data.Uvs;
            renderers = new TorusQuadRenderer[longitudes * latitudes];

            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                GameObject        rendererObj       = new GameObject("Renderer " + longitude + ":" + latitude);
                Transform         rendererTransform = rendererObj.transform;
                TorusQuadRenderer renderer          = rendererObj.AddComponent<TorusQuadRenderer>();

                rendererObj.isStatic = true;
                
                rendererTransform.SetParent(transform);
                rendererTransform.localPosition = Vector3.zero;

                renderer.longitude        = longitude;
                renderer.latitude         = latitude;
                renderer.parentController = this;
                
                renderer.GenerateMeshes();

                renderers[longitude * latitudes + latitude] = renderer;
                
                renderer.SetCollider(physicMaterial);
            }

            AdjustSeamNormals();
            ApplyMaterials();
            SaveMeshes();
            SaveMaterials();
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SaveMaterials()
        {
            PrefabStage        prefabStage  = PrefabStageUtility.GetCurrentPrefabStage();
            UnityEngine.Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            string prefabPath = prefabStage != null ?
                prefabStage.assetPath :
                AssetDatabase.GetAssetPath(prefabSource);

            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                Transform child = transform.GetChild(longitude * latitudes + latitude);
                foreach (Transform grandChild in child)
                {
                    Material mat = grandChild.GetComponent<MeshRenderer>().sharedMaterial;
                    AssetDatabase.AddObjectToAsset(mat, prefabPath);
                }
            }
        }

        private void AdjustSeamNormals()
        {
            Vector3[][][] normalArrays = new Vector3[renderers.Length][][]; // renderer, lod, vertex

            for (int i = 0; i < renderers.Length; i++)
                normalArrays[i] = renderers[i].GetAdjustedNormalArrays();

            for (int i = 0; i < renderers.Length; i++)
                renderers[i].ApplyNormalArrays(normalArrays[i]);
        }

        private void SaveMeshes()
        {
            PrefabStage        prefabStage  = PrefabStageUtility.GetCurrentPrefabStage();
            Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            string prefabPath = prefabStage != null ?
                prefabStage.assetPath :
                AssetDatabase.GetAssetPath(prefabSource);

            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                Transform child = transform.GetChild(longitude * latitudes + latitude);
                foreach (Transform grandChild in child)
                {
                    Mesh               mesh   = grandChild.GetComponent<MeshFilter>().sharedMesh;
                    AssetDatabase.AddObjectToAsset(mesh, prefabPath);
                }
            }
        }

# endif
    }
    
    public class TorusMaterialInfo
    {
        public Shader    Shader;
        public Vector2   TileSize;
        public int       ColsPerQuad;
        public bool      UseHeightmapOffset;
        public Vector2   Offset;
        public Texture2D Splatmap0;
        public Texture2D Layer0;
        public Texture2D Layer1;
        public Texture2D Layer2;
        public Texture2D Layer3;
    }
}