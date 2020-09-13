using System;

namespace UI.Animation
{
    public abstract class ReversibleAnimation : Animation, IReversible
    {
        public abstract override void Play(float duration, Action onCompleteCallback = null);
        public abstract void Reverse(float duration, Action onCompleteCallback = null);
    }
}