using System;
using UnityEngine;

namespace UI.Animation
{
    public abstract class Animation : MonoBehaviour, IPlayable
    {
        public GameObject target;

        public virtual void Awake()
        {
            if (target == null)
                target = gameObject;
        }

        public abstract void Play(float duration, Action onCompleteCallback = null);
    }
}