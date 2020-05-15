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
        private static readonly string[] Strings =
        {
            "Singleplayer",
            "Freeplay",
            "Classic Tag"
        };

        public override string ToString() => Strings[Index];
    }
}