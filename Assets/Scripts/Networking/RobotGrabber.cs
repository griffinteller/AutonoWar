using Photon.Pun;
using UnityEngine;

namespace Networking
{
    public class RobotGrabber : MonoBehaviour
    {

        public PlayerConnection playerConnection;

        public void Update()
        {

            var currentRobots = GameObject.FindGameObjectsWithTag("Robot");
            foreach (var robot in currentRobots)
            {

                playerConnection.robots[robot.GetComponent<PhotonView>().Owner.ActorNumber] = robot;

            }

            if (playerConnection.robots.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {

                Debug.Log("Fully Loaded!");
                Debug.Log(playerConnection.robots);
                playerConnection.OnFullyLoaded();
                enabled = false;

            }

        }
    }
}