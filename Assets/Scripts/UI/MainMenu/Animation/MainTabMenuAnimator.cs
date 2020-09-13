using System;
using UnityEngine;

namespace UI.MainMenu.Animation
{
    [RequireComponent(typeof(MenuAnimator))]
    public class MainTabMenuAnimator : MonoBehaviour
    {
        private MenuAnimator _menuAnimator;
        
        public void Start()
        {
            _menuAnimator = GetComponent<MenuAnimator>();
            
            var buttonsParent = FindObjectOfType<TopBarButtons>();
            buttonsParent.onSelect[transform.GetSiblingIndex()].AddListener(_menuAnimator.Enter);
            buttonsParent.onDeselect[transform.GetSiblingIndex()].AddListener(_menuAnimator.Exit);
            
            if (buttonsParent.SelectedIndex != transform.GetSiblingIndex())
                gameObject.SetActive(false);
        }
    }
}