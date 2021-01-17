using UnityEditor;
using UnityEngine;
namespace Editor.Scripts
{
    [CustomEditor(typeof(GameTerrain.old.QuadTerrainHeightmap))]
    public class OldQuadHeightmapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GameTerrain.old.QuadTerrainHeightmap heightmap = (GameTerrain.old.QuadTerrainHeightmap) target;
            
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                heightmap.Generate();
            }
        }
    }
}