using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class ModalWindow : MonoBehaviour
    {

        public CanvasGroup canvasGroup;

        [SerializeField] private Text messageText;
        [SerializeField] private UiUtility uiUtility;

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        public void Open()
        {
            uiUtility.ShowModalWindow(canvasGroup);
        }

        public void Close()
        {
            uiUtility.HideModalWindow(canvasGroup);
        }

    }
}