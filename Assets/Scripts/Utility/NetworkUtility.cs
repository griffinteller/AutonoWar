using System;
using Photon.Pun;
using Photon.Realtime;

namespace Utility
{
    public static class NetworkUtility
    {
        public static Player[] PlayerArrayByActorNumber()
        {
            var playerDict = PhotonNetwork.CurrentRoom.Players;
            var result = new Player[playerDict.Count];
            
            playerDict.Values.CopyTo(result, 0);
            Array.Sort(result, (player1, player2) => player1.ActorNumber.CompareTo(player2.ActorNumber));

            return result;
        }

        public static int[] ActorNumberArray()
        {
            var players = PhotonNetwork.CurrentRoom.Players;
            var result = new int[players.Count];
            
            players.Keys.CopyTo(result, 0);
            Array.Sort(result);

            return result;
        }

        public static int GetPlayerIndex(int actorNumber)
        {
            var actorNumbers = ActorNumberArray();
            return Array.IndexOf(actorNumbers, actorNumber);
        }
    }
}