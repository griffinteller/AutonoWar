using System;
using ExitGames.Client.Photon;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Main
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

            if (photonEvent.Code != PhotonEvent.NewItCode)
            {
                return;
            }
            
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
                
                playerConnection.robots[lastItActorNumber].transform.Find("Body").Find("Base").gameObject
                    .GetComponent<MeshRenderer>().material.color = normalTint;
                playerConnection.robots[currentItActorNumber].transform.Find("Body").Find("Base").gameObject
                    .GetComponent<MeshRenderer>().material.color = itTint;
                _lastTag = Time.time;
                
            }
            

        }

        public void RaiseNewItEvent(int actorNumber)
        {

            var eventCode = PhotonEvent.NewItCode;
            var content = new [] {actorNumber};
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, sendOptions);

        }

        private void OnCollisionEnter(Collision other)
        {

            var collisionObjectRoot = other.transform.root;
            if (collisionObjectRoot.tag.Equals("Robot") && 
                PhotonNetwork.LocalPlayer.ActorNumber == currentItActorNumber)
            {

                Debug.Log("Collided with a robot!");
                RaiseNewItEvent(collisionObjectRoot.GetComponent<RobotNetworkBridge>().actorNumber);

            }    

        }

    }
}