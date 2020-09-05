using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.GameSelection
{
    public class ImageOption : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;

        private ImageOptionPanel _parentPanel;
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                _parentPanel.SetSelected();
            }
        }

        public Image Image
        {
            get => image;
            set => image = value;
        }

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public void Awake()
        {
            _parentPanel = transform.parent.GetComponent<ImageOptionPanel>();
        }
    }
}