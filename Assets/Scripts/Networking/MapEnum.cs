using System;
using Utility;

namespace Networking
{
    public enum MapEnum
    {
        Highlands
    }

    [Serializable]
    public class MapEnumWrapper : EnumWrapper
    {
        private static readonly string[] Strings =
        {
            "Highlands"
        };

        public override string ToString() => Strings[Index];
    }
}