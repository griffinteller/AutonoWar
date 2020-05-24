using System;
using GameDirection;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Main
{

    public class RobotMain : MonoBehaviour
    {
        private static bool _showingBeacons;
        
        public int robotIndex;

        private const float MaxAngularVelocity = 120f;
        private const float TagBuffer = 0.3f;
        private const string BeaconName = "Beacon";
        private RobotNetworkBridge _robotNetworkBridge;
        private SinglePlayerDirector _singlePlayerDirector;
        private GameObject _beaconObject;
        private GameDirector _gameDirector;
        private bool _partsAreLoaded;
        private bool _shouldColor;
        private Color _newColor;
        
        private const float TintCombineAmount = 0.9f;

        public void OnPartsLoaded()
        {
            _partsAreLoaded = true;
            GetComponent<ActionHandler>().LoadTiresIntoDict();
            GetComponent<EasySuspension>().enabled = true;
            AddSphereTrigger();

            if (_shouldColor)
                CombineTint(_newColor);
        }

        public void CombineTint(Color tint, bool undo = false)
        {
            if (!_partsAreLoaded)
            {
                _shouldColor = true;
                _newColor = tint;
            }

            float t;
            if (undo)
            {
                t = 1f - 1f / (1f - TintCombineAmount);
            }
            // undoes combine;
            else
            {
                t = TintCombineAmount;
            }

            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                var originalColor = meshRenderer.material.color;
                meshRenderer.material.color = Color.LerpUnclamped(originalColor, tint, t);
            }
        }

        public void Start()
        {
            SetMaximumAngularVelocities();
            InitializeScripts();

            _beaconObject = transform.Find(BeaconName).gameObject;
            _gameDirector = GameObject.FindGameObjectWithTag("GameDirector").GetComponent<GameDirector>();
        }

        private void AddSphereTrigger()
        {

            var bounds = new Bounds();
            var colliders = GetComponentsInChildren<Collider>();
            
            foreach (var collider in colliders)
                bounds.Encapsulate(collider.bounds.extents + collider.transform.localPosition);

            var sphereCollider = gameObject.AddComponent<SphereCollider>();

            sphereCollider.radius = bounds.extents.magnitude + TagBuffer;
            sphereCollider.isTrigger = true;

        }

        private void InitializeScripts()
        {
            if (PhotonNetwork.InRoom)
            {
                _robotNetworkBridge = GetComponent<RobotNetworkBridge>();
                _robotNetworkBridge.enabled = true;
            }
            else
            {
                InitializeSinglePlayerScripts();
            }
        }

        public void Update()
        {
            KeyCheck();
            TryShowBeacons();
        }

        private void TryShowBeacons()
        {
            if ((_robotNetworkBridge && !_robotNetworkBridge.isLocal) 
                || (_singlePlayerDirector 
                    && !(_singlePlayerDirector.SelectedRobot == robotIndex)))
                
                SetBeaconActive(_showingBeacons);

            else
                SetBeaconActive(false);
        }

        private void SetBeaconActive(bool show)
        {
            _beaconObject.SetActive(show);
        }

        public void ToggleBeaconActive()
        {
            var beacon = transform.Find(BeaconName).gameObject;
            beacon.SetActive(!beacon.activeSelf);
        }

        private void InitializeSinglePlayerScripts()
        {
            _singlePlayerDirector = FindObjectOfType<SinglePlayerDirector>();
            
            GetComponent<UserScriptInterpreter>().enabled = true;
            GetComponent<RobotStateSender>().enabled = true;
            GetComponent<DesignLoaderPlay>().enabled = true;
            
            GetComponent<DesignLoaderPlay>().BuildRobot();
        }

        private void SetMaximumAngularVelocities()
        {
            var rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.maxAngularVelocity = MaxAngularVelocity;
            }
        }
        
        private void KeyCheck()
        {
            if (Input.GetKeyDown(KeyCode.Q) && robotIndex == 0)
                _showingBeacons = !_showingBeacons;
        }

        public void OnTriggerEnter(Collider other)
        {
            var collisionRoot = other.transform.root.gameObject;
            if (_gameDirector.GameMode == GameModeEnum.ClassicTag && collisionRoot.CompareTag("Robot"))
            {
                var tagDirector = (ClassicTagDirector) _gameDirector;
                Debug.Log("Collided with a robot!");
                if (_robotNetworkBridge.actorNumber == tagDirector.currentItActorNumber)
                    tagDirector.TryRaiseNewItEvent(collisionRoot.GetComponent<RobotNetworkBridge>().actorNumber);
            }
        }
    }
}