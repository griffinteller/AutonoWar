using System;
using UnityEngine;

namespace UI.MainMenu.Animation
{
    public class MoveAnchors : MenuItemAnimation
    {
        public Rect onScreen;
        public Rect offScreen;

        private RectTransform _rectTransform;

        public void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void Enter(float duration, Action onCompleteCallback = null)
        {
            LeanTween.value(gameObject, UpdateAnchors, 0, 1, duration).setOnComplete(onCompleteCallback);
        }

        public void UpdateAnchors(float currentValue)
        {
            _rectTransform.anchorMin = Vector2.Lerp(offScreen.min, onScreen.min, currentValue);
            _rectTransform.anchorMax = Vector2.Lerp(offScreen.max, onScreen.max, currentValue);
        }

        public override void Exit(float duration, Action onCompleteCallback = null)
        {
            LeanTween.value(gameObject, UpdateAnchors, 1, 0, duration).setOnComplete(onCompleteCallback);
        }
    }
}