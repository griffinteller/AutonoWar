using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility
{
    public static class MetaUtility
    {

        public static T[] GetComponentsInProperChildren<T> (GameObject obj)
            where T : Component
        {

            var raw = obj.GetComponentsInChildren<T>();
            var result = new List<T>();
            foreach (var component in raw)
            {
                if (obj.GetInstanceID() != component.gameObject.GetInstanceID())
                    result.Add(component);
            }

            return result.ToArray();

        }

        public static UnityEvent UnityEventFromFunc(Action func)
        {
            var result = new UnityEvent();
            result.AddListener(new UnityAction(func));
            return result;
        }

        public static Button.ButtonClickedEvent UnityEventToButtonClickedEvent(UnityEvent unityEvent)
        {
            var result = new Button.ButtonClickedEvent();
            result.AddListener(unityEvent.Invoke);
            return result;
        }
        
        
    }
}