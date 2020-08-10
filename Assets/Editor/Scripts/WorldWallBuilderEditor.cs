using UnityEditor;
using UnityEngine;
using Utility;

namespace Editor.Scripts
{
    [CustomEditor(typeof(WorldWallBuilder))]
    public class WorldWallBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Build"))
                ((WorldWallBuilder) target).BuildWalls();
        }
    }
}