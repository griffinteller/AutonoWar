using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI.MainMenu.RobotDisplay
{
    [RequireComponent(typeof(ScrollRect))]
    public class PropertyList : MonoBehaviour
    {
        public float verticalMargins;
        public float sideMargins;
        public float barHeight;
        public Color barColor;

        public PropertyListEntry entryPrefab;

        private List<(string, string)> _data;
        private RectTransform _contentTransform;

        public void Start()
        {
            _contentTransform = GetComponent<ScrollRect>().content;
        }

        public void SetData(List<(string, string)> newData)
        {
            if (!_contentTransform)
                Start();
            
            _data = newData;
            MetaUtility.DestroyAllChildren(_contentTransform);
            for (var i = 0; i < _data.Count; i++)
            {
                var entry = Instantiate(entryPrefab.gameObject, _contentTransform).GetComponent<PropertyListEntry>();
                entry.name = _data[i].Item1;
                entry.keyText.text = _data[i].Item1;
                entry.valueText.text = _data[i].Item2;
                entry.transform.SetParent(_contentTransform);
                entry.transform.localScale = Vector3.one;
                entry.AdjustSize(verticalMargins, sideMargins);
                entry.transform.SetSiblingIndex(i * 2);

                if (i == _data.Count - 1)
                    break;

                var bar = new GameObject("Bar" + i, typeof(Image)).GetComponent<Image>();

                var barTransform = bar.GetComponent<RectTransform>();
                barTransform.SetParent(_contentTransform);
                barTransform.localScale = Vector3.one;
                barTransform.localPosition = Vector3.zero;

                bar.color = barColor;
                var barSize = bar.rectTransform.sizeDelta;
                barSize.y = barHeight;
                barSize.x = 0;
                barTransform.sizeDelta = barSize;
                barTransform.SetSiblingIndex(i * 2 + 1);
            }

            _contentTransform.anchoredPosition = Vector3.zero;
        }
    }
}