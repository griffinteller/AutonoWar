using System;
using UnityEngine;

namespace UI.MainMenu.Animation
{
    public abstract class MenuItemAnimation : MonoBehaviour
    {
        public abstract void Enter(float duration, Action onCompleteCallback = null);
        public abstract void Exit(float duration, Action onCompleteCallback = null);
    }
}