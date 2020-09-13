using System;
using UnityEngine;

namespace UI.Animation
{
    public abstract class Animation : MonoBehaviour, IPlayable
    {
        public abstract void Play(float duration, Action onCompleteCallback = null);
    }
}