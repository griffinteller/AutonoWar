using System;
using UnityEngine;

namespace UI.Animation
{
    public class LinearTranslation : ReversibleAnimation
    {
        public Vector3 localDelta;
        
        public override void Play(float duration, Action onCompleteCallback = null)
        {
            LeanTween
                .moveLocal(target, target.transform.localPosition + localDelta, duration)
                .setOnComplete(onCompleteCallback);
        }

        public override void Reverse(float duration, Action onCompleteCallback = null)
        {
            LeanTween
                .moveLocal(target, target.transform.localPosition - localDelta, duration)
                .setOnComplete(onCompleteCallback);
        }
    }
}