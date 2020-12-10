using UnityEditor;
using GameTerrain;
using UnityEngine;

namespace Editor.Scripts
{
    [CustomEditor(typeof(QuadTerrainHeightmap))]
    public class QuadHeightmapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                ((QuadTerrainHeightmap) target).Generate();
            }

        }
    }
}