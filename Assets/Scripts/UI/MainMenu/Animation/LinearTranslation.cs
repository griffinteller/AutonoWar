using System;
using UnityEngine;

namespace UI.MainMenu.Animation
{
    public class LinearTranslation : MenuItemAnimation
    {
        public Vector3 localDelta;
        public override void Enter(float duration, Action onCompleteCallback = null)
        {
            var animation = LeanTween.moveLocal(gameObject, transform.localPosition - localDelta, duration);
            
            if (onCompleteCallback != null)
                animation.setOnComplete(onCompleteCallback);
        }

        public override void Exit(float duration, Action onCompleteCallback = null)
        {
            var animation = LeanTween.moveLocal(gameObject, transform.localPosition + localDelta, duration);
            
            if (onCompleteCallback != null)
                animation.setOnComplete(onCompleteCallback);
        }
    }
}