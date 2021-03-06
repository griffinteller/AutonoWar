﻿using System;
using System.Collections.Generic;
using Cam;
using GameDirection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utility;

namespace Networking
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        private GameDirector _gameDirector;
        public CameraMotionScript cameraMotionScript;

        public ClassicTagDirector classicTagDirector;
        public FreeplayDirector freeplayDirector;
        public GrandPrixDirector grandPrixDirector;
        [HideInInspector] public GameModeEnum gameMode;
        [HideInInspector] public GameObject playerObject;

        public GameObject playerObjectPrefab;
        public Dictionary<int, Rigidbody> robotRigidbodies = new Dictionary<int, Rigidbody>();

        public Dictionary<int, GameObject> robots = new Dictionary<int, GameObject>();
        [HideInInspector] public PositionRotationPair startingPositionAndRotation;

        public override void OnEnable()
        {
            base.OnEnable();
            
            if (!PhotonNetwork.InRoom)
            {
                DestroyImmediate(gameObject);
                return;
            }

            gameMode = (GameModeEnum) PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
            InstantiateGameDirector();

            startingPositionAndRotation =
                _gameDirector.GetStartingPositionsAndRotations()[PhotonNetwork.LocalPlayer.ActorNumber];

            var playerObj =
                PhotonNetwork.Instantiate(
                    playerObjectPrefab.name, 
                    startingPositionAndRotation.position,
                    startingPositionAndRotation.rotation);
            
            cameraMotionScript.SetCenterObject(playerObj);
        }
        
        public static void SetRobotsKinematic(bool isKinematic)
        {
            foreach (var rigidbody in FindObjectOfType<PlayerConnection>().robotRigidbodies.Values)
                rigidbody.isKinematic = isKinematic;
        }

        private void InstantiateGameDirector()
        {
            switch (gameMode)
            {
                case GameModeEnum.ClassicTag:

                    _gameDirector = Instantiate(classicTagDirector.gameObject).GetComponent<ClassicTagDirector>();
                    break;

                case GameModeEnum.FreePlay:

                    _gameDirector = Instantiate(freeplayDirector.gameObject).GetComponent<FreeplayDirector>();
                    break;
                
                case GameModeEnum.GrandPrix:
                    
                    _gameDirector = Instantiate(grandPrixDirector.gameObject).GetComponent<GrandPrixDirector>();
                    break;

                default:

                    throw new NotImplementedException();
            }

            _gameDirector.CurrentMap = (MapEnum) PhotonNetwork.CurrentRoom.CustomProperties["map"];
        }

        public void OnFullyLoaded()
        {
            _gameDirector.FullyLoaded = true;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            robots.Remove(otherPlayer.ActorNumber);
        }
    }
}