using System;

namespace UI
{
    [Serializable]
    public class ScoreboardColumn
    {
        public readonly string Name;
        public readonly bool IsFloat;
        public readonly byte DecimalPlaces;
        public readonly object InitialValue;
        public readonly bool Expand;
        public readonly float DefaultWidth;

        public ScoreboardColumn(string name, object initialValue = null, bool isFloat = false, byte decimalPlaces = 0,
            bool expand = false, float defaultWidth = 30)
        {
            Name = name;
            IsFloat = isFloat;
            DecimalPlaces = decimalPlaces;
            
            if (initialValue == null)
            {
                if (isFloat)
                    initialValue = 0f;
                else
                    initialValue = "";
            }

            InitialValue = initialValue;
            Expand = expand;
            DefaultWidth = defaultWidth;
        }
    }
}