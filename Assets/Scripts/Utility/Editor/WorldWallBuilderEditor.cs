using UnityEditor;
using UnityEngine;

namespace Utility.Editor
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