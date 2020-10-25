using System.Collections.Generic;

namespace UI
{
    public class ColoredString
    {
        private Dictionary<string, int> _colorIndices = new Dictionary<string, int>();
        private int[] _colorIndexArray;
        
        public string RawString { get; }
        
    }
}