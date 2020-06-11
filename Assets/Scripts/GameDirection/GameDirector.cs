using System.Collections.Generic;
using ExitGames.Client.Photon;
using Main;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEngine;
using Utility;

namespace GameDirection
{
    public enum GameState : byte
    {
        Loading = 0,
        Initializing,
        Started,
        Ended,
        WaitingForTransition
    }
    public abstract class GameDirector : MonoBehaviourPunCallbacks
    {
        public abstract GameModeEnum GameMode { get; }

        public GameState gameState;

        public bool FullyLoaded
        {
            get => _fullyLoaded;
            set
            {
                _fullyLoaded = value;
                if (_fullyLoaded)
                    OnFullyLoaded();
            }
        }

        private bool _fullyLoaded;

        protected virtual void OnFullyLoaded() {}

        public virtual Dictionary<int, Vector3> GetStartingPositions(Vector3 center)
        {
            const float radius = 10f;
            const float distanceOffGround = 1f;
            var players = PhotonNetwork.CurrentRoom.Players;
            
            var playersSorted = new List<Player>();
            foreach (var pair in players)
            {
                playersSorted.Add(pair.Value);
            }
            playersSorted.Sort(
                Comparer<Player>.Create((x, y) => x.ActorNumber.CompareTo(y.ActorNumber))
                );

            var result = new Dictionary<int, Vector3>();
            for (var i = 0; i < playersSorted.Count; i++)
            {
                var player = playersSorted[i];
                
                var pos = center;
                pos.x += radius * Mathf.Cos(2 * Mathf.PI / players.Count * i);
                pos.z += radius * Mathf.Sin(2 * Mathf.PI / players.Count * i);
                pos.y = TerrainUtility.GetClosestCurrentTerrain(center).SampleHeight(center) + distanceOffGround;

                result.Add(player.ActorNumber, pos);
            }

            return result;
        }

        public virtual List<EscapeMenuButtonInfo> GetEscapeMenuButtonInfo()
        {
            return new List<EscapeMenuButtonInfo>
            {
                new EscapeMenuButtonInfo("Main Menu", MetaUtility.UnityEventFromFunc(HudUi.ReturnToMainMenu))
            };
        } 

        protected void RaiseStartGameEvent()
        {
            RaiseEventDefaultSettings(PhotonEventCode.StartingGame);
            gameState = GameState.WaitingForTransition;
        }

        protected void RaiseEndGameEvent()
        {
            RaiseEventDefaultSettings(PhotonEventCode.EndingGame);
            gameState = GameState.WaitingForTransition;
        }

        public static void RaiseEventDefaultSettings(PhotonEventCode eventCode, object data = null)
        {
            var content = data;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte) eventCode, content, raiseEventOptions, sendOptions);
        }

        public static void ResetLocalRobot()
        {
            var playerConnectionObj = GameObject.FindWithTag("ConnectionObject");
            
            if (playerConnectionObj)
            {
                var playerConnection = playerConnectionObj.GetComponent<PlayerConnection>();
                playerConnection.playerObject.GetComponent<ActionHandler>().ResetRobot();
            }
            else
            {
                GameObject.FindWithTag("GameDirector").GetComponent<SinglePlayerDirector>()
                    .GetSelectedRobot().GetComponent<ActionHandler>().ResetRobot();
            }
        }
    }
} 