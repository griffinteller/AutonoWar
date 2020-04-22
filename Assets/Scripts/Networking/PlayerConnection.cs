using System;
using System.Collections.Generic;
using Main;
using Photon.Pun;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Networking
{
    public class PlayerConnection : MonoBehaviour
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
            Debug.Log(startingPosition);
            
            var res = PhotonNetwork.Instantiate(playerObjectPrefab.name, startingPosition, Quaternion.identity);
            cameraMotionScript.SetCenterObject(res);

        }

        public void OnFullyLoaded()
        {

            if (PhotonNetwork.CurrentRoom.CustomProperties["Gamemode"].Equals("Classic Tag"))
            {

                robots[PhotonNetwork.LocalPlayer.ActorNumber].GetComponent<ClassicTagScript>().enabled = true;

            }
            
        }

    }
}
