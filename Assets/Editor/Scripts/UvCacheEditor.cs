using GameTerrain;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    [CustomEditor(typeof(UvCache))]
    public class UvCacheEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Generate"))
                ((UvCache) target).GenerateCache();
        }
    }
}