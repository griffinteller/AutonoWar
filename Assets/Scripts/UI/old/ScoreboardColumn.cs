using System;

namespace UI
{
    [Serializable]
    public class ScoreboardColumn
    {
        public string name;
        public bool expand;
        public float defaultWidth;
        public bool isSerialized;
        public string defaultValue;
        public bool isInt;
        public bool isFloat;

        public ScoreboardColumn(string name)
        {
            this.name = name;
        }
    }
}