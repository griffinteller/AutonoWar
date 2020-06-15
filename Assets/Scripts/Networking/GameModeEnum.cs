using Utility;

namespace Networking
{
    public enum GameModeEnum
    {
        SinglePlayer,
        FreePlay,
        ClassicTag
    }

    public class GameModeEnumWrapper : EnumWrapper
    {
        protected override string[] Strings => new[]
        {
            "Singleplayer",
            "Freeplay",
            "Classic Tag"
        };
    }
}