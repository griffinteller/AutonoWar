using GameTerrain;
using UnityEditor;
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
            
            if (GUILayout.Button("Generate"))
                ((TorusTerrainController) target).Generate();

            if (GUILayout.Button("Remove All Renderers"))
                MetaUtility.DestroyImmediateAllChildren(((TorusTerrainController) target).transform);
        }
    }
}