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
            QuadTerrainHeightmap heightmap = (QuadTerrainHeightmap) target;
            
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                heightmap.Generate();
            }
            
            GUILayout.Label("Current stored heightmap length: " + heightmap.heightsStored.Length);
        }
    }
}