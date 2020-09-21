using System;
using UnityEngine;

namespace UI.Animation
{
    public class MoveAnchors : ReversibleAnimation
    {
        public Rect start;
        public Rect finish;

        private RectTransform _rectTransform;

        public override void Awake()
        {
            base.Awake();
            _rectTransform = target.GetComponent<RectTransform>();
        }

        public override void Play(float duration, Action onCompleteCallback = null)
        {
            LeanTween
                .value(target, UpdateAnchors, 0, 1, duration)
                .setOnComplete(onCompleteCallback);
        }

        private void UpdateAnchors(float currentValue)
        {
            _rectTransform.anchorMin = Vector2.Lerp(start.min, finish.min, currentValue);
            _rectTransform.anchorMax = Vector2.Lerp(start.max, finish.max, currentValue);
        }

        public override void Reverse(float duration, Action onCompleteCallback = null)
        {
            LeanTween
                .value(target, UpdateAnchors, 1, 0, duration)
                .setOnComplete(onCompleteCallback);
        }
    }
}