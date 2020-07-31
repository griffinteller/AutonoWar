using UnityEditor;
using UnityEngine;

namespace GamePhysics.Editor
{
    public class FrictionCurveCreator : EditorWindow
    {
        public const string FolderForCurves = "Assets/Scripts/GamePhysics/FrictionCurves/";
        
        public float x1 = 0.5f;
        public float y1 = 0.95f;
        public float x2 = 1.5f;
        public float y2 = 1.1f;
        public float x3 = 3f;
        public float y3 = 0.7f;
        public float stiffness = 1;

        public bool showIntegrationSettings;
        
        public int relativeTorqueSamples = 50;
        public float asymptoteDifferenceThreshold = 0.01f;
        public int slipSamples = 100;

        public string curveName = "NewFrictionCurve";
        
        [MenuItem("Window/Friction Curve Creator")]
        static void Init()
        {
            var window = (FrictionCurveCreator) GetWindow(typeof(FrictionCurveCreator));
            window.Show();
        }


        public void OnGUI()
        {
            x1 = EditorGUILayout.FloatField("x1", x1);
            y1 = EditorGUILayout.FloatField("y1", y1);
            x2 = EditorGUILayout.FloatField("x2", x2);
            y2 = EditorGUILayout.FloatField("y2", y2);
            x3 = EditorGUILayout.FloatField("x3", x3);
            y3 = EditorGUILayout.FloatField("y3", y3);
            stiffness = EditorGUILayout.FloatField("Stiffness", stiffness);

            EditorGUILayout.Space(10);
            
            showIntegrationSettings = 
                EditorGUILayout.BeginFoldoutHeaderGroup(showIntegrationSettings, "Integration Settings");

            if (showIntegrationSettings)
            {
                relativeTorqueSamples = EditorGUILayout.IntField("Torque Samples", relativeTorqueSamples);
                asymptoteDifferenceThreshold = EditorGUILayout.FloatField("Asymptote Difference Threshold",
                    asymptoteDifferenceThreshold);
                slipSamples = EditorGUILayout.IntField("Slip Samples", slipSamples);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space(10);

            curveName = EditorGUILayout.TextField("Curve Name", curveName);

            if (GUILayout.Button("Save"))
            {
                var frictionCurve = CreateInstance<FrictionCurve>();

                frictionCurve.x1 = x1;
                frictionCurve.y1 = y1;
                frictionCurve.x2 = x2;
                frictionCurve.y2 = y2;
                frictionCurve.x3 = x3;
                frictionCurve.y3 = y3;
                frictionCurve.stiffness = stiffness;

                frictionCurve.relativeTorqueSamples = relativeTorqueSamples;
                frictionCurve.asymptoteDifferenceThreshold = asymptoteDifferenceThreshold;
                frictionCurve.slipSamples = slipSamples;
                
                frictionCurve.Setup();

                AssetDatabase.CreateAsset(frictionCurve, FolderForCurves + curveName + ".asset");
            }
        }
    }
}