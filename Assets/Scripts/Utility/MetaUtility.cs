using System.Collections.Generic;
using UnityEngine;

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
        
    }
}