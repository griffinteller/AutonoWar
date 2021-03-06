﻿using System;
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
        public virtual Dictionary<MapEnum, Vector3> BaseStartingPositions => new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(-280, 0, -410)},
            {MapEnum.Desert, new Vector3(98, 0, -130)}
        };

        private bool _fullyLoaded;

        public abstract GameModeEnum GameMode { get; }
        public virtual HashSet<HudElement> HudElements => new HashSet<HudElement>();
        public virtual MapEnum[] AllowedMaps => MapEnumWrapper.DefaultMaps;
        public MapEnum CurrentMap { get; set; }
        public PlayerConnection playerConnection;

        public void Awake()
        {
            playerConnection = FindObjectOfType<PlayerConnection>();
        }

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

        protected virtual void OnFullyLoaded()
        {
        }

        public virtual Dictionary<int, PositionRotationPair> GetStartingPositionsAndRotations()
        {
            const float radius = 10f;
            const float distanceOffGround = 1f;
            var center = BaseStartingPositions[CurrentMap];
            var players = PhotonNetwork.CurrentRoom.Players;

            var playersSorted = NetworkUtility.PlayerArrayByActorNumber();

            var result = new Dictionary<int, PositionRotationPair>();
            for (var i = 0; i < playersSorted.Length; i++)
            {
                var player = playersSorted[i];
                var resultElement = new PositionRotationPair();

                var pos = center;
                pos.x += radius * Mathf.Cos(2 * Mathf.PI / players.Count * i);
                pos.z += radius * Mathf.Sin(2 * Mathf.PI / players.Count * i);
                pos.y = TerrainUtility.GetClosestCurrentTerrain(pos).SampleHeight(pos) + distanceOffGround;
                
                resultElement.position = pos;
                resultElement.rotation = Quaternion.Euler(0, 360f / players.Count * i - 90, 0);

                result.Add(player.ActorNumber, resultElement);
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

        public static void RaiseEventDefaultSettings(PhotonEventCode eventCode, object data = null)
        {
            var content = data;
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
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