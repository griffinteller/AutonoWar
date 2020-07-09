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
                var actorNumber = robot.GetComponent<PhotonView>().Owner.ActorNumber;

                playerConnection.robots[actorNumber] = robot;
                playerConnection.robotRigidbodies[actorNumber] = robot.GetComponent<Rigidbody>();
            }

            if (playerConnection.robots.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Debug.Log("Fully Loaded!");
                enabled = false;
                playerConnection.OnFullyLoaded();
            }
        }
    }
}