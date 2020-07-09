using Utility;

namespace Networking
{
    public enum GameModeEnum
    {
        SinglePlayer,
        FreePlay,
        ClassicTag,
        GrandPrix
    }

    public class GameModeEnumWrapper : EnumWrapper
    {
        protected override string[] Strings => new[]
        {
            "Singleplayer",
            "Freeplay",
            "Classic Tag",
            "Grand Prix"
        };
    }
}