using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI.MainMenu.Animation
{
    public class MenuAnimator : MonoBehaviour
    {
        public float baseDelay;
        public List<MenuItemAnimation> otherAnimations = new List<MenuItemAnimation>();

        public UnityEvent onEnter;
        public UnityEvent onExit;

        private List<MenuItemAnimation> childAnimations;

        public void Start()
        {
            var buttonsParent = FindObjectOfType<TopBarButtons>();
            buttonsParent.onSelect[transform.GetSiblingIndex()].AddListener(Enter);
            buttonsParent.onDeselect[transform.GetSiblingIndex()].AddListener(Exit);
            
            childAnimations = new List<MenuItemAnimation>(GetComponentsInChildren<MenuItemAnimation>());
            
            foreach (var animation in otherAnimations)
                childAnimations.Add(animation);
            
            if (buttonsParent.SelectedIndex != transform.GetSiblingIndex())
                gameObject.SetActive(false);
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            foreach (var animation in childAnimations)
                animation.Enter(baseDelay);
            
            onEnter.Invoke();
        }

        public void Exit()
        {
            if (childAnimations.Count == 0)
                gameObject.SetActive(false);
            
            for (var i = 0; i < childAnimations.Count; i++)
            {
                var animation = childAnimations[i];
                if (i == childAnimations.Count - 1)
                    animation.Exit(baseDelay, delegate { gameObject.SetActive(false); }); // disable menu once done
                else
                    animation.Exit(baseDelay);
            }
            
            onExit.Invoke();
        }
    }
}