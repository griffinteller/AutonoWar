using System;
using UnityEngine;

namespace UI.Animation
{
    public class LinearTranslation : ReversibleAnimation
    {
        public Vector3 localDelta;
        
        public override void Play(float duration, Action onCompleteCallback = null)
        {
            var animation = LeanTween.moveLocal(gameObject, transform.localPosition - localDelta, duration);
            
            if (onCompleteCallback != null)
                animation.setOnComplete(onCompleteCallback);
        }

        public override void Reverse(float duration, Action onCompleteCallback = null)
        {
            var animation = LeanTween.moveLocal(gameObject, transform.localPosition + localDelta, duration);
            
            if (onCompleteCallback != null)
                animation.setOnComplete(onCompleteCallback);
        }
    }
}