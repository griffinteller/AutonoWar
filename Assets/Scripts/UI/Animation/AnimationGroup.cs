using System;

namespace UI.Animation
{
    public class AnimationGroup : GenericAnimationGroup<Animation>
    {
        public override void Play(float duration, Action onCompleteCallback = null)
        {
            if (animations.Count == 0)
            {
                onCompleteCallback?.Invoke();
                return;
            }

            for (var i = 0; i < animations.Count - 1; i++)
                animations[i].Play(duration);
            
            animations[animations.Count - 1].Play(duration, onCompleteCallback);
        }
    }
}