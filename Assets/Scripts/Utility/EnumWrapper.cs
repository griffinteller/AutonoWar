using System;

namespace Utility
{
    public abstract class EnumWrapper
    {
        protected abstract string[] Strings { get; }
        public int Index { get; set; }

        public override string ToString()
        {
            return Strings[Index];
        }

        protected int IndexFromStringInternal(string str)
        {
            return Array.IndexOf(Strings, str);
        }
    }
}