using System;
using UI.Animation;
using UnityEngine;
using UnityEngine.Events;

namespace UI.MainMenu.Animation
{
    [RequireComponent(typeof(ReversibleAnimationGroup))]
    public class MainTabMenuAnimator : MonoBehaviour
    {
        private ReversibleAnimationGroup _animationGroup;

        public float duration;
        public UnityEvent onEnter;
        public UnityEvent onExit;
        
        public void Start()
        {
            _animationGroup = GetComponent<ReversibleAnimationGroup>();
            
            var buttonsParent = FindObjectOfType<TopBarButtons>();
            buttonsParent.onSelect[transform.GetSiblingIndex()].AddListener(Enter);
            buttonsParent.onDeselect[transform.GetSiblingIndex()].AddListener(Exit);
            
            if (buttonsParent.SelectedIndex != transform.GetSiblingIndex())
                gameObject.SetActive(false);
        }

        protected void Enter()
        {
            gameObject.SetActive(true);
            _animationGroup.Play(duration);
            onEnter.Invoke();
        }

        protected void Exit()
        {
            _animationGroup.Reverse(duration, delegate { gameObject.SetActive(false); });
            onExit.Invoke();
        }
    }
}