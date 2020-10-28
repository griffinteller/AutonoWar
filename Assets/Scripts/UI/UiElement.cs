using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UiElement : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        private CanvasGroup _canvasGroup;

        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;

                if (ReferenceEquals(_canvasGroup, null))
                    _canvasGroup = GetComponent<CanvasGroup>();
                
                _canvasGroup.alpha = value ? 1 : 0;
                _canvasGroup.interactable = value;
                _canvasGroup.blocksRaycasts = value;
            } 
        }

        public void OnEnable()
        {
            Visible = visible;
        }
    }
}