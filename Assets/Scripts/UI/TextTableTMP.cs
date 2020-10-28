using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TextTableTMP : MonoBehaviour
    {
        public TMP_Text text;
        public int color;
        public int color2;
        public string[] columns;
        public string[] data;

        private TableTextGen _tableTextGen;

        public void Start()
        {
            ColoredString[] coloredColumns = ColoredString.ColorStringArray(columns, color);
            ColoredString[] coloredStrings = ColoredString.ColorStringArray(data, color2);
            _tableTextGen = new TableTextGen(coloredColumns, color, -1, 
                new List<ColoredString[]> {coloredStrings});
            text.text = _tableTextGen.Generate();
        }
    }
}