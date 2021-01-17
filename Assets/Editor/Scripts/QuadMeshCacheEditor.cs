using GameTerrain;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    [CustomEditor(typeof(QuadMeshCache))]
    public class QuadMeshCacheEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Generate"))
                ((QuadMeshCache) target).Generate();
        }
    }
}