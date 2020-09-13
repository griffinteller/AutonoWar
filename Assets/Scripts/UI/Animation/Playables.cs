using System;

namespace UI.Animation
{
    public interface IPlayable
    {
        void Play(float duration, Action onCompleteCallback = null);
    }

    public interface IReversible : IPlayable
    {
        void Reverse(float duration, Action onCompleteCallback = null);
    }
}