using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class TopBarButtons : MonoBehaviour
    {
        public Color selectedColor;
        public Color unselectedColor;
        
        public Color selectedTextColor;
        public Color unselectedTextColor;

        public List<UnityEvent> onDeselect;
        public List<UnityEvent> onSelect;

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
            
            onDeselect = new List<UnityEvent>();
            onSelect = new List<UnityEvent>();
            foreach (var _ in _buttons)
            {
                onDeselect.Add(new UnityEvent());
                onSelect.Add(new UnityEvent());
            }

        }

        private void SelectNew(Button button)
        {
            var newIndex = button.transform.GetSiblingIndex();

            if (newIndex == SelectedIndex)
                return;
            
            onDeselect[SelectedIndex].Invoke();
            SelectedIndex = newIndex;
            onSelect[SelectedIndex].Invoke();
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