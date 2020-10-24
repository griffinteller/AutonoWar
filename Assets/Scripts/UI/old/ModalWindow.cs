using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalWindow : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        [SerializeField] private Text messageText;
        [SerializeField] private UiUtility uiUtility;

        public void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        public void Open()
        {
            uiUtility.ShowModalWindow(_canvasGroup);
        }

        public void Close()
        {
            uiUtility.HideModalWindow(_canvasGroup);
        }
    }
}