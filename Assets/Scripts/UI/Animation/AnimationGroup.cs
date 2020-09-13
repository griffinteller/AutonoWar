using System;

namespace UI.Animation
{
    public class AnimationGroup : GenericAnimationGroup<Animation>
    {
        public override void Play(float duration, Action onCompleteCallback = null)
        {
            foreach (var animation in animations)
                animation.Play(duration, onCompleteCallback);
        }
    }
}