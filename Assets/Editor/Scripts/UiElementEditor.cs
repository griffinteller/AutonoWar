using System;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Scripts
{
    [CustomEditor(typeof(UiElement))]
    public class UiElementEditor : UnityEditor.Editor
    {
        public void OnEnable()
        {
            ((UiElement) target).GetComponent<CanvasGroup>().hideFlags = HideFlags.HideInInspector;
        }

        public override void OnInspectorGUI()
        {
            UiElement uiElement = (UiElement) target;

            bool visible = uiElement.Visible;
            bool shouldBeVisible = GUILayout.Toggle(visible, "Enabled");

            if (visible != shouldBeVisible)
            {
                uiElement.Visible = shouldBeVisible;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}