using Building;
using Cam;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    // goes on UI canvas. Watches for tires that are placed down.
    public class TireNameInputHandler : MonoBehaviour
    {
        private BuildTireComponent _buildTireComponent;

        public BuildHandler buildHandler;
        public CameraMotionScript cameraMotionScript;

        public CanvasGroup tireNamingWindow;

        public void Start()
        {
            CloseInputWindow();
        }

        public void CloseInputWindow()
        {
            UiUtility.DisableCanvasGroupStatic(tireNamingWindow);

            SetSceneInteractable(true);
        }

        public void ShowInputWindow(BuildTireComponent tireComponentJustPlaced)
        {
            _buildTireComponent = tireComponentJustPlaced;

            UiUtility.EnableCanvasGroup(tireNamingWindow);

            SetSceneInteractable(false);
        }

        public void SetTireName(Text textComponent)
        {
            SetTireName(textComponent.text);
        }

        private void SetTireName(string tireName)
        {
            _buildTireComponent.tireName = tireName;
            CloseInputWindow();
        }

        private void SetSceneInteractable(bool interactable)
        {
            cameraMotionScript.interactable = interactable;
            buildHandler.interactable = interactable;
        }
    }
}