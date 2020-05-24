using System;
using System.Collections.Generic;
using Cam;
using GameDirection;
using Main;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utility;

using Random = UnityEngine.Random;

namespace Networking
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {

        public GameObject playerObjectPrefab;
        [HideInInspector] public GameObject playerObject;
        public Vector3 startingPosition;
        public CameraMotionScript cameraMotionScript;
        public Dictionary<int, GameObject> robots;
        public ClassicTagDirector classicTagDirector;
        public FreeplayDirector freeplayDirector;
        [HideInInspector] public GameModeEnum gameMode;

        private GameDirector _gameDirector;

        public void Start()
        {
            robots = new Dictionary<int, GameObject>();
            
            startingPosition.x += Random.Range(-10, 10);
            startingPosition.z += Random.Range(-10, 10);
            startingPosition.y = TerrainUtility.GetClosestCurrentTerrain(startingPosition).SampleHeight(startingPosition) + 1;

            var res = PhotonNetwork.Instantiate(playerObjectPrefab.name, startingPosition, Quaternion.identity);
            cameraMotionScript.SetCenterObject(res);
            gameMode = (GameModeEnum) PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
            
            InstantiateGameDirector();
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
