using System;

namespace Utility
{
    public abstract class EnumWrapper
    {
        public int Index { get; set; }
        public abstract override string ToString();
    }
}