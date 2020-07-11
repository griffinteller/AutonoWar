using Building;
using Cam;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class RobotNameHandler : MonoBehaviour
    {
        public BuildHandler buildHandler;
        public CameraMotionScript cameraMotionScript;

        public CanvasGroup namingWindow;

        public void Start()
        {
            CloseInputWindow();
        }

        public void CloseInputWindow()
        {
            UiUtility.DisableCanvasGroupStatic(namingWindow);
            SetSceneInteractable(true);
        }

        public void ShowInputWindow()
        {
            UiUtility.EnableCanvasGroup(namingWindow);
            SetSceneInteractable(false);
        }

        public void SetRobotName(Text textComponent)
        {
            SetRobotName(textComponent.text);
        }

        private void SetRobotName(string robotName)
        {
            buildHandler.SaveDesign(robotName);
            CloseInputWindow();
        }

        private void SetSceneInteractable(bool interactable)
        {
            cameraMotionScript.interactable = interactable;
            buildHandler.interactable = interactable;
        }
    }
}