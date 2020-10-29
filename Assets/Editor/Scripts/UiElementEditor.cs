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
        private UiElement _uiElement;
        public void OnEnable()
        {
            _uiElement = (UiElement) target;
            _uiElement.GetComponent<CanvasGroup>().hideFlags = HideFlags.HideInInspector;
            SetPickability(_uiElement.gameObject, _uiElement.Visible);
        }

        public override void OnInspectorGUI()
        {
            bool visible = _uiElement.Visible;
            bool shouldBeVisible = GUILayout.Toggle(visible, "Enabled");

            if (visible != shouldBeVisible)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                _uiElement.Visible = shouldBeVisible;
                EditorSceneManager.MarkSceneDirty(activeScene);

                SetPickability(_uiElement.gameObject, shouldBeVisible);
            }
        }

        public static void SetPickability(GameObject obj, bool pickable)
        {
            if (pickable)
                SceneVisibilityManager.instance.EnablePicking(obj, true);
            else
                SceneVisibilityManager.instance.DisablePicking(obj, true);
        }
    }
}