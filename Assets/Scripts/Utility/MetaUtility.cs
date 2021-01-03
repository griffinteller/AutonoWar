using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility
{
    public static class MetaUtility
    {
        public static T[] GetComponentsInProperChildren<T>(GameObject obj)
            where T : Component
        {
            var raw = obj.GetComponentsInChildren<T>();
            var result = new List<T>();
            foreach (var component in raw)
                if (obj.GetInstanceID() != component.gameObject.GetInstanceID())
                    result.Add(component);

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

        public static void SyncRectTransforms(RectTransform source, RectTransform toBeChanged)
        {
            toBeChanged.anchorMax = source.anchorMax;
            toBeChanged.anchorMin = source.anchorMin;
            toBeChanged.pivot = source.pivot;
            toBeChanged.anchoredPosition = source.anchoredPosition;
            toBeChanged.localScale = source.localScale;
            toBeChanged.sizeDelta = source.sizeDelta;
        }

        public static void DestroyAllChildren(Transform transform)
        {
            foreach (Transform child in transform)
                UnityEngine.Object.Destroy(child.gameObject);
        }
        
        public static void DestroyAllChildren(GameObject gameObject)
        {
            DestroyAllChildren(gameObject.transform);
        }
        
        public static void DestroyImmediateAllChildren(Transform transform)
        {
            foreach (Transform child in transform)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        
        public static void DestroyImmediateAllChildren(GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        public static Vector3 Vector3Abs(Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.y));
        }

        public static bool TryDispose<T>(this NativeArray<T> array) 
            where T : struct
        {
            bool disposable = array.IsCreated;

            if (disposable)
                array.Dispose();

            return disposable;
        }
    }
}