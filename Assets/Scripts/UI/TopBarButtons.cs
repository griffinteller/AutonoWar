using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class TopBarButtons : MonoBehaviour
    {
        public Color selectedColor;
        public Color unselectedColor;
        
        public Color selectedTextColor;
        public Color unselectedTextColor;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetSelectedIndex(value);
        }

        private int _selectedIndex;
        private List<Button> _buttons = new List<Button>();

        public void Start()
        {
            foreach (Transform child in transform)
            {
                var button = child.GetComponent<Button>();
                button.onClick.AddListener(delegate { SelectNew(button); });
                _buttons.Add(button);
            }
        }

        private void SelectNew(Button button)
        {
            SelectedIndex = button.transform.GetSiblingIndex();
        }

        private void SetSelectedIndex(int index)
        {
            _selectedIndex = index;
            SetAllButtonColors(unselectedColor);
            SetAllTextColors(unselectedTextColor);
            _buttons[index].image.color = selectedColor;
            _buttons[index].GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        }

        private void SetAllTextColors(Color color)
        {
            foreach (var button in _buttons)
                button.GetComponentInChildren<TMP_Text>().color = color;
        }

        private void SetAllButtonColors(Color color)
        {
            foreach (var button in _buttons)
                button.image.color = color;
        }
    }
}