using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TextTable : MonoBehaviour
    {
        private TableTextGen _tableTextGen = new TableTextGen();
        
        [SerializeField] private Color tableColor;
        [SerializeField] private TMP_Text text;

        public Color TableColor
        {
            get => tableColor;
            set
            {
                tableColor = value;
                _tableTextGen.color = ColoredString.HexFromColor(tableColor);
            }
        }

        public ColoredString[] ColumnTitles
        {
            get => _tableTextGen.columns;
            set => _tableTextGen.columns = value;
        }

        public List<ColoredString[]> Data
        {
            get => _tableTextGen.data;
            set => _tableTextGen.data = value;
        }

        public void Start()
        {
            RefreshText();
        }

        public void RefreshText()
        {
            text.text = _tableTextGen.Generate();
        }
    }
}