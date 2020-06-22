using System;
using UnityEngine;

namespace Utility
{
    [Serializable]
    public class SerializableColor
    {
        private float[] _values;

        public SerializableColor(Color color)
        {
            _values = new[] {color.r, color.g, color.b, color.a};
        }

        public Color GetColor()
        {
            return new Color(_values[0],_values[1], _values[2], _values[3]);
        }

        public static implicit operator Color(SerializableColor sColor) => sColor.GetColor();
        public static implicit operator SerializableColor(Color color) => new SerializableColor(color);
    }
}