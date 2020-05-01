using Main;
using Photon.Pun;
using UnityEngine;

namespace Networking
{
    
    public class RobotNetworkBridge : MonoBehaviourPun
    {

        public GameObject playerConnectionObject;
        public PlayerConnection playerConnection;
        public int actorNumber;

        public void Start()
        {

            Debug.Log("Starting Robot Network Bridge");
            
            playerConnectionObject = GameObject.FindWithTag("ConnectionObject");
            playerConnection = playerConnectionObject.GetComponent<PlayerConnection>();

            actorNumber = photonView.Owner.ActorNumber;

            if (photonView.IsMine)
            {
                    
                playerConnection.playerObject = gameObject;
                    
                Camera.main.GetComponent<CameraMotionScript>().SetCenterObject(gameObject);
                    
                GetComponent<UserScriptInterpreter>().enabled = true;
                GetComponent<RobotStateSender>().enabled = true;

            }

        }

    }
}
