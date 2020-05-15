using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Networking
{
    public class ClassicTagScript : MonoBehaviourPun, IOnEventCallback
    {

        public int currentItActorNumber = -1;
        public int lastItActorNumber = -1;

        private PlayerConnection playerConnection;

        public Color itTint;
        public Color normalTint;

        public float tagCooldown = 2;

        private float _lastTag;

        public void OnEnable()
        {

            _lastTag = -tagCooldown;
            PhotonNetwork.AddCallbackTarget(this);
            
        }

        public void OnDisable()
        {
            
            PhotonNetwork.RemoveCallbackTarget(this);
            
        }

        public void OnEvent(EventData photonEvent)
        {

            if (photonEvent.Code != (byte) PhotonEventCode.NewIt)
                return;

            SetNewIt(((int[]) photonEvent.CustomData)[0]);
            
        }

        public void Start()
        {
            
            Debug.Log("Starting Tag!");
            var connectionObject = GameObject.FindWithTag("ConnectionObject");
            playerConnection = connectionObject.GetComponent<PlayerConnection>();
            
            currentItActorNumber = PhotonNetwork.MasterClient.ActorNumber;
            SetNewIt(currentItActorNumber);

        }
        
        public void SetNewIt(int actorNumber)
        {

            if (Time.time - _lastTag > tagCooldown)
            {
                
                Debug.Log("Setting new it to " + PhotonNetwork.CurrentRoom.Players[actorNumber].NickName);

                lastItActorNumber = currentItActorNumber;
                currentItActorNumber = actorNumber;

                var oldItMeshRenderers =
                    playerConnection.robots[lastItActorNumber].GetComponentsInChildren<MeshRenderer>();
                var newItMeshRenderers =
                    playerConnection.robots[currentItActorNumber].GetComponentsInChildren<MeshRenderer>();

                foreach (var meshRenderer in oldItMeshRenderers)
                {
                    
                    meshRenderer.material.color = normalTint;

                }

                foreach (var meshRenderer in newItMeshRenderers)
                {

                    meshRenderer.material.color = itTint;

                }
                
                _lastTag = Time.time;
                
            }
            

        }

        public void RaiseNewItEvent(int actorNumber)
        {

            var eventCode = PhotonEventCode.NewIt;
            var content = new [] {actorNumber};
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte) eventCode, content, raiseEventOptions, sendOptions);

        }

        private void OnCollisionEnter(Collision other)
        {

            var collisionObjectRoot = other.transform.root;
            if (collisionObjectRoot.tag.Equals("RobotDescription") && 
                PhotonNetwork.LocalPlayer.ActorNumber == currentItActorNumber)
            {

                Debug.Log("Collided with a robot!");
                RaiseNewItEvent(collisionObjectRoot.GetComponent<RobotNetworkBridge>().actorNumber);

            }    

        }

    }
}