using System;

namespace UI.Animation
{
    public class ReversibleAnimationGroup : GenericAnimationGroup<ReversibleAnimation>, IReversible
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

        public void Reverse(float duration, Action onCompleteCallback = null)
        {
            if (animations.Count == 0)
            {
                onCompleteCallback?.Invoke();
                return;
            }

            for (var i = 0; i < animations.Count - 1; i++)
                animations[i].Reverse(duration);
            
            animations[animations.Count - 1].Reverse(duration, onCompleteCallback);
        }

        public void PlayFromEditor(float duration)
        {
            Play(duration);
        }

        public void ReverseFromEditor(float duration)
        {
            Reverse(duration);
        }
    }
}