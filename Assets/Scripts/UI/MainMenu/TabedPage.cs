using System;
using UnityEngine;
using UnityEngine.Events;

namespace UI.MainMenu
{
    public class TabedPage : MonoBehaviour
    {
        public TopBarButtons topBarButtons;
        public int index;
        public UnityEvent onSelect;
        public UnityEvent onDeselect;

        public void Start()
        {
            topBarButtons.onDeselect[index] = onDeselect;
            topBarButtons.onSelect[index] = onSelect;
        }
    }
}