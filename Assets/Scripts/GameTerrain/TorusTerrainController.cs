using System;
using UnityEngine;

# if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

# endif

namespace GameTerrain
{
    public class TorusTerrainController : MonoBehaviour
    {
        public int      majorRadius;
        public int      minorRadius;
        public int      longitudes;
        public int      latitudes;
        public float[]  lodViewPercentages; // degree = 7 - lod
        public int[]    lodQuadDegrees;
        public Material material;

        [Header("Info, do not edit from editor")] 
        
        public float majorQuadArc; // degrees
        public float minorQuadArc;

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
        public float         lightmapScale = 0.003f;

        [NonSerialized] public float[]     Heights;
        [NonSerialized] public int[][]     Triangles;
        [NonSerialized] public Vector2[][] Uvs;

        public void Generate()
        {
            Awake();

            Heights = heightmap.LoadHeightmap();
            QuadMeshCacheData data = quadMeshCache.Deserialize();
            Triangles = data.Triangles;
            Uvs       = data.Uvs;

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
            }

            SaveMeshes();
        }

        private void SaveMeshes()
        {
            for (int longitude = 0; longitude < longitudes; longitude++)
            for (int latitude = 0; latitude < latitudes; latitude++)
            {
                Transform child = transform.GetChild(longitude * latitudes + latitude);
                foreach (Transform grandChild in child)

                {
                    Mesh               mesh   = grandChild.GetComponent<MeshFilter>().sharedMesh;
                    string prefabPath = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
                    AssetDatabase.AddObjectToAsset(mesh, prefabPath);
                }
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
# endif
    }
}