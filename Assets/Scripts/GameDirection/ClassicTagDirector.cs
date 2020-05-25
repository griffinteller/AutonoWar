using ExitGames.Client.Photon;
using Main;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GameDirection
{
    public class ClassicTagDirector : GameDirector, IOnEventCallback
    {
        public override GameModeEnum GameMode => GameModeEnum.ClassicTag;

        public int currentItActorNumber = -1;
        public int lastItActorNumber = -1;

        public Color itTint = Color.red;

        public float tagCooldownTime = 5;

        private float _lastTagTime;
        private PlayerConnection _playerConnection;

        public void OnEnable()
        {
            _lastTagTime = -tagCooldownTime;
            _playerConnection = GameObject.FindGameObjectWithTag("ConnectionObject")
                .GetComponent<PlayerConnection>();
            
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

            _lastTagTime = Time.time;
            print("reveived new it event!");
            SetNewIt(((int[]) photonEvent.CustomData)[0]);
        }

        public void TryRaiseNewItEvent(int actorNumber)
        {
            if (Time.time - _lastTagTime < tagCooldownTime || !FullyLoaded)
                return;

            _lastTagTime = Time.time;
            const PhotonEventCode eventCode = PhotonEventCode.NewIt;
            var content = new [] {actorNumber};
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            print("Raising It Event!");
            PhotonNetwork.RaiseEvent((byte) eventCode, content, raiseEventOptions, sendOptions);
        }

        protected override void OnFullyLoaded()
        {
            print("Setting initial it");
            currentItActorNumber = PhotonNetwork.MasterClient.ActorNumber;
            lastItActorNumber = currentItActorNumber;

            CombineTintWithRobot(currentItActorNumber, itTint);
        }

        private void SetNewIt(int actorNumber)
        {
            lastItActorNumber = currentItActorNumber;
            currentItActorNumber = actorNumber;

            CombineTintWithRobot(currentItActorNumber, itTint);
            CombineTintWithRobot(lastItActorNumber, itTint, true); // reset to old color
        }

        private void CombineTintWithRobot(int actorNumber, Color tint, bool undo = false)
        {
            var robot = _playerConnection.robots[actorNumber];

            robot.GetComponent<RobotMain>().CombineTint(tint, undo);
        }

        /*public int currentItActorNumber = -1;
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

        public void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return;
            
            var collisionObjectRoot = other.transform.root;
            if (collisionObjectRoot.tag.Equals("Robot") && 
                PhotonNetwork.LocalPlayer.ActorNumber == currentItActorNumber)
            {

                Debug.Log("Collided with a robot!");
                RaiseNewItEvent(collisionObjectRoot.GetComponent<RobotNetworkBridge>().actorNumber);

            }    
            
        }*/

        
    }
}