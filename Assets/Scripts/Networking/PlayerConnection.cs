using System.Collections.Generic;
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
        public GameObject playerObject;
        public Vector3 startingPosition;
        public CameraMotionScript cameraMotionScript;
        public Dictionary<int, GameObject> robots;

        public void Start()
        {
            
            robots = new Dictionary<int, GameObject>();
            
            startingPosition.x += Random.Range(-10, 10);
            startingPosition.z += Random.Range(-10, 10);
            startingPosition.y = TerrainUtility.GetClosestCurrentTerrain(startingPosition).SampleHeight(startingPosition) + 1;

            var res = PhotonNetwork.Instantiate(playerObjectPrefab.name, startingPosition, Quaternion.identity);
            cameraMotionScript.SetCenterObject(res);

        }

        public void OnFullyLoaded()
        {

            if ((GameModeEnum) PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == GameModeEnum.ClassicTag)
            {

                robots[PhotonNetwork.LocalPlayer.ActorNumber].GetComponent<ClassicTagScript>().enabled = true;

            }
            
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {

            robots.Remove(otherPlayer.ActorNumber);

        }
    }
}
