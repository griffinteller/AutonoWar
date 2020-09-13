using System;

namespace UI.Animation
{
    public class ReversibleAnimationGroup : GenericAnimationGroup<ReversibleAnimation>, IReversible
    {
        public override void Play(float duration, Action onCompleteCallback = null)
        {
            foreach (var animation in animations)
                animation.Play(duration, onCompleteCallback);
        }

        public void Reverse(float duration, Action onCompleteCallback = null)
        {
            foreach (var animation in animations)
                animation.Reverse(duration, onCompleteCallback);
        }
    }
}