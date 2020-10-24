using UnityEngine;
using UnityEngine.Playables;

namespace UI.Animation
{
    public class AnimationGroup : MonoBehaviour, IAnimation
    {
        public IAnimation[] animations;
        
        public void Play(float duration)
        {
            foreach (IAnimation animation in animations)
                animation.Play(duration);
        }
    }
}