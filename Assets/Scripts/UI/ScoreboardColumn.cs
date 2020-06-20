namespace UI
{
    public class ScoreboardColumn
    {
        public readonly string Name;
        public readonly bool IsFloat;
        public readonly byte DecimalPlaces;
        public readonly object InitialValue;

        public ScoreboardColumn(string name, object initialValue = null, bool isFloat = false, byte decimalPlaces = 0)
        {
            Name = name;
            IsFloat = isFloat;
            DecimalPlaces = decimalPlaces;
            
            if (initialValue == null)
            {
                if (isFloat)
                    initialValue = 0;
                else
                    initialValue = "";
            }

            InitialValue = initialValue;
        }
    }
}