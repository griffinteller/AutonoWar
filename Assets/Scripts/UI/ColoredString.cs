using System;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

namespace UI
{
    public struct ColoredChar
    {
        public char rawChar;
        public Color color;

        public ColoredChar(char rawChar, Color? color = null)
        {
            this.rawChar = rawChar;
            this.color = color ?? Color.black;
        }
    }
    
    public class ColoredString
    {
        private Dictionary<Color, int> _colorIndices; // we rename each color with an index, to save memory
        private Dictionary<int, Color> _indexColors; // the reverse of _colorIndices, for color-lookup
        private int[] _colorIndexArray; // the colors of each character, with the color index

        public string RawString { get; }
        public int Length => RawString.Length;

        private ColoredString(string rawString, Dictionary<Color, int> colorIndices,
            Dictionary<int, Color> indexColors, int[] colorIndexArray)
        {
            RawString = rawString;
            _colorIndices = colorIndices;
            _indexColors = indexColors;
            _colorIndexArray = colorIndexArray;
        }

        public ColoredString(string str, Color? defaultColor = null, params (Color, (int, int))[] ranges)
        {
            RawString = str;
            _colorIndices = new Dictionary<Color, int>
            {
                {defaultColor ?? Color.black, 0}
            };
            _indexColors = new Dictionary<int, Color>
            {
                {0, defaultColor ?? Color.black}
            };
            _colorIndexArray = new int[str.Length];

            for (int rangeIndex = 0; rangeIndex < ranges.Length; rangeIndex++)
            {
                Color color = ranges[rangeIndex].Item1;
                (int start, int end) = ranges[rangeIndex].Item2;

                int colorIndex;
                if (_colorIndices.ContainsKey(color))
                    colorIndex = _colorIndices[color];
                else
                {
                    colorIndex = _colorIndices.Count;
                    _colorIndices.Add(color, colorIndex);
                    _indexColors.Add(colorIndex, color);
                }
                for (int charIndex = start; charIndex < end; charIndex++)
                    _colorIndexArray[charIndex] = colorIndex;
            }
        }

        public static ColoredString operator + (ColoredString str1, ColoredString str2)
        {
            return Concat(str1, str2);
        }

        public static ColoredString Concat(ColoredString str1, ColoredString str2)
        {
            ColoredString smallStr = str1.Length < str2.Length ? str1 : str2;
            ColoredString largeStr = smallStr == str1 ? str2 : str1;

            string newRawString = str1.RawString + str2.RawString;
            Dictionary<Color, int> newColorIndices = largeStr._colorIndices;
            Dictionary<int, Color> newIndexColors = largeStr._indexColors;
            
            // we need to make sure both string use the same color-index pairings
            // its more efficient to switch the small string over to the "language" of the large string
            int[] newSmallStrIndexArray = smallStr._colorIndexArray;
            for (int i = 0; i < newSmallStrIndexArray.Length; i++)
            {
                // switch to the new pairing
                newSmallStrIndexArray[i] = newColorIndices[smallStr._indexColors[smallStr._colorIndexArray[i]]];
            }

            int[] newColorIndexArray = new int[newRawString.Length];
            if (smallStr == str1)
            {
                newSmallStrIndexArray.CopyTo(newColorIndexArray, 0);
                str2._colorIndexArray.CopyTo(newColorIndexArray, str1.Length);
            }
            else
            {
                str1._colorIndexArray.CopyTo(newColorIndexArray, 0);
                newSmallStrIndexArray.CopyTo(newColorIndexArray, str1.Length);
            }
            
            return new ColoredString(newRawString, newColorIndices, newIndexColors, newColorIndexArray);
        }

        public ColoredChar CharAt(int index)
        {
            return new ColoredChar(
                RawString.Substring(index, 1).ToCharArray()[0], 
                _indexColors[_colorIndexArray[index]]);
        }

        public ColoredString Substring(int startIndex, int length)
        {
            string rawString = RawString.Substring(startIndex, length);
            int[] newIndexArray = new int[length];
            Array.Copy(_colorIndexArray, startIndex,
                newIndexArray, 0, length);
            
            // TODO: Optimize memory by removing unused colors in new string from the dictionaries
            return new ColoredString(rawString, _colorIndices, _indexColors, newIndexArray);
        }
    }
}