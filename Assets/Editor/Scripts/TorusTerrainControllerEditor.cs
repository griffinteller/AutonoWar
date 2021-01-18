using GameTerrain;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using Utility;

namespace Editor.Scripts
{
    [CustomEditor(typeof(TorusTerrainController))]
    public class TorusTerrainControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TorusTerrainController terrainController = (TorusTerrainController) target;
            
            if (GUILayout.Button("Generate All"))
                terrainController.Generate();

            if (GUILayout.Button("Remove Sub-Assets"))
            {
                MetaUtility.DestroyImmediateAllChildren(terrainController.transform);
                
                PrefabStage        prefabStage  = PrefabStageUtility.GetCurrentPrefabStage();
                Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(terrainController.gameObject);

                string prefabPath = prefabStage != null ?
                    prefabStage.assetPath :
                    AssetDatabase.GetAssetPath(prefabSource);

                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
                
                foreach (Object asset in allAssets)
                    if (asset is Mesh || asset is Material)
                        DestroyImmediate(asset, true);

                EditorUtility.SetDirty(obj);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Generate Materials"))
            {
                terrainController.GenerateMaterials();
            }
                
        }
    }
}