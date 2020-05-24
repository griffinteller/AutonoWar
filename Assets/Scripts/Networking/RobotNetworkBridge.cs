using Building;
using Cam;
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
        public bool isLocal;

        public void Start()
        {

            Debug.Log("Starting RobotDescription Network Bridge");
            
            playerConnectionObject = GameObject.FindWithTag("ConnectionObject");
            playerConnection = playerConnectionObject.GetComponent<PlayerConnection>();

            actorNumber = photonView.Owner.ActorNumber;

            if (photonView.IsMine)
            {

                isLocal = true;
                playerConnection.playerObject = gameObject;
                    
                Camera.main.GetComponent<CameraMotionScript>().SetCenterObject(gameObject);

                var robotStructure = BuildHandler.GetRobotStructure();
                photonView.RPC(
                    "BuildRobotRpc", 
                    RpcTarget.AllBuffered, 
                    robotStructure.ToJson());
                    
                GetComponent<UserScriptInterpreter>().enabled = true;
                GetComponent<RobotStateSender>().enabled = true;
            }

        }

    }
}
