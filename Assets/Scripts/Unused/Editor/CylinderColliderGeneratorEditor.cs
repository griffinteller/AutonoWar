using UnityEditor;
using UnityEngine;
using Unused;

namespace Editor
{
    [CustomEditor(typeof(CylinderColliderGenerator))]
    public class CylinderColliderGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var baseComponent = (CylinderColliderGenerator) target;
            
            if (GUILayout.Button("Build Colliders With Spheres"))
                baseComponent.BuildCylinderSpheres();
            
            if (GUILayout.Button("Build Colliders With Boxes"))
                baseComponent.BuildCylinderBoxes();
        }
    }
}