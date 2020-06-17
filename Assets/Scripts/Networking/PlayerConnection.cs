using System;
using System.Collections.Generic;
using Cam;
using GameDirection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Networking
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        private GameDirector _gameDirector;
        public CameraMotionScript cameraMotionScript;

        public ClassicTagDirector classicTagDirector;
        public FreeplayDirector freeplayDirector;
        [HideInInspector] public GameModeEnum gameMode;
        [HideInInspector] public GameObject playerObject;

        public GameObject playerObjectPrefab;
        public Dictionary<int, Rigidbody> robotRigidbodies = new Dictionary<int, Rigidbody>();

        public Dictionary<int, GameObject> robots = new Dictionary<int, GameObject>();
        [HideInInspector] public Vector3 startingPosition;

        public void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                DestroyImmediate(gameObject);
                return;
            }

            gameMode = (GameModeEnum) PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
            InstantiateGameDirector();

            startingPosition =
                _gameDirector.GetStartingPositions()[PhotonNetwork.LocalPlayer.ActorNumber];
            print(startingPosition);
            print(PhotonNetwork.LocalPlayer.ActorNumber);

            var playerObj =
                PhotonNetwork.Instantiate(playerObjectPrefab.name, startingPosition, Quaternion.identity);
            cameraMotionScript.SetCenterObject(playerObj);
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