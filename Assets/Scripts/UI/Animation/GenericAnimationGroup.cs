using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UI.Animation
{
    public abstract class GenericAnimationGroup<T> : Animation
        where T : Animation
    {
        protected readonly List<T> animations = new List<T>();
        
        [SerializeField] private List<T> nonChildAnimations;

        public bool findChildAnimations;
        
        public void Start()
        {
            foreach (var animation in nonChildAnimations)
                animations.Add(animation);

            if (!findChildAnimations)
                return;
            
            foreach (var animation in MetaUtility.GetComponentsInProperChildren<T>(gameObject))
                animations.Add(animation);
        }
    }
}