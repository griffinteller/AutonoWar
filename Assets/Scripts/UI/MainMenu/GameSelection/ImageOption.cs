using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.GameSelection
{
    [RequireComponent(typeof(Button))]
    public class ImageOption : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image highlight;

        private ImageOptionPanel _parentPanel;
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                var newValue = _parentPanel.SetSelected(this, value);
                _selected = newValue;
                highlight.gameObject.SetActive(_selected);
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

        public void Start()
        {
            _parentPanel = transform.GetComponentInParent<ImageOptionPanel>();
            GetComponent<Button>().onClick.AddListener(delegate { Selected = !Selected; });
        }
    }
}