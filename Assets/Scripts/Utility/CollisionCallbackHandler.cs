using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class CollisionCallbackHandler : MonoBehaviour
    {
        public UnityAction<Collision> enterCallbacks;

        public void OnCollisionEnter(Collision other)
        {
            enterCallbacks.Invoke(other);
        }
    }
}