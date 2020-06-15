using System;

namespace Networking
{
    [Serializable]
    public class ServerDescription
    {
        public int gameLength;
        public GameModeEnum gameMode;
        public MapEnum map;
        public byte maxPlayers;
        public string name;
    }
}