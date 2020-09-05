using System;

namespace UI.Scoreboard
{
    [Serializable]
    public class ScoreboardColumn
    {
        public readonly string Name;
        public readonly bool IsFloat;
        public readonly byte DecimalPlaces;
        public readonly object InitialValue;
        public readonly CellLayout Layout;

        public ScoreboardColumn(string name, object initialValue = null, bool isFloat = false, byte decimalPlaces = 0,
            CellLayout cellLayout = null)
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
            Layout = cellLayout ?? CellLayout.Default();
        }
    }
}