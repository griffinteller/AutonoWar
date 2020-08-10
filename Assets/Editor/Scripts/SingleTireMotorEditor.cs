using System;
using GamePhysics.Tire;
using UnityEditor;

namespace Editor.Scripts
{
    [CustomEditor(typeof(SingleTireMotor))]
    public class SingleTireMotorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (SingleTireMotor) target;
            DrawDefaultInspector();
            EditorGUILayout.TextArea("Power: " + component.Power);
            EditorGUILayout.TextArea("Motor Torque: " + component.MotorTorque);
            EditorGUILayout.TextArea("Brake Torque: " + component.BrakeTorque);
            EditorGUILayout.TextArea("Angular Velocity: " + component.attachedTireComponent.AngularVelocity);
        }
    }
}
