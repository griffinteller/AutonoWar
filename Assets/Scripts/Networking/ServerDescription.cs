using System;
using Utility;

namespace Networking
{
    [Serializable]
    public class ServerDescription
    {

        public string name;
        public GameModeEnum gameMode;
        public byte maxPlayers;
        public MapEnum map;
        public int gameLength;
    }
}