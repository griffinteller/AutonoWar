using Networking;
using Photon.Pun;
using UnityEngine;

namespace GameDirection
{
    public class GrandPrixEndPoint : MonoBehaviour
    {
        public void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
                Destroy(this);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            var otherRoot = other.transform.root;
            if (!otherRoot.CompareTag("Robot")) 
                return;
            
            var actorNumber = otherRoot.GetComponent<RobotNetworkBridge>().actorNumber;
            FindObjectOfType<GrandPrixDirector>().RobotHasFinished(actorNumber);
        }
    }
}