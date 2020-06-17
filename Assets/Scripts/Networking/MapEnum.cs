using System;
using System.Collections.Generic;
using Utility;

namespace Networking
{
    public enum MapEnum
    {
        Highlands,
        Desert
    }

    [Serializable]
    public class MapEnumWrapper : EnumWrapper
    {
        public static readonly MapEnum[] DefaultMaps =
        {
            MapEnum.Highlands,
            MapEnum.Desert
        };
        
        public static readonly Dictionary<MapEnum, string> MapSceneNames = new Dictionary<MapEnum, string>
        {
            {MapEnum.Highlands, "HighlandsScene"},
            {MapEnum.Desert, "DesertScene"}
        };

        protected override string[] Strings => new[]
        {
            "Highlands",
            "Desert"
        };
    }
}