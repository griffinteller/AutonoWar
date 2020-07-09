using System;
using System.Collections.Generic;
using UnityEngine;
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
        
        public static readonly Dictionary<MapEnum, Vector3> MapSizes = new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(4096, 10000, 4096)},
            {MapEnum.Desert, new Vector3(4096, 10000, 4096)}
        };

        protected override string[] Strings => new[]
        {
            "Highlands",
            "Desert"
        };
    }
}