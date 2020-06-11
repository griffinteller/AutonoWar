using System;
using System.Runtime.InteropServices;
using GameDirection;
using Networking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main
{

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ScheduledFlip))]
    public class RobotMain : MonoBehaviour
    {
        private static bool _showingBeacons;
        
        public static Action<Collider, RobotMain> OnTriggerEnterCallbacks = (collider, robotMain) => {};
        
        public int robotIndex;
        
        [FormerlySerializedAs("_robotNetworkBridge")] public RobotNetworkBridge robotNetworkBridge;

        private const float MaxAngularVelocity = 120f;
        private const float TagBuffer = 0.3f;
        private const string BeaconName = "Beacon";
        
        private SinglePlayerDirector _singlePlayerDirector;
        private GameObject _beaconObject;
        private GameDirector _gameDirector;
        private bool _partsAreLoaded;
        private bool _shouldColor;
        private Color _newColor;
        private Transform _robotBody;
        private Rigidbody _rigidbody;
        private ScheduledFlip _scheduledFlipComponent;

        private const float TintCombineAmount = 0.9f;
        private const float MinStuckAngularVelocity = 1f;

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
            print("Combining color. Undo = " + undo);
            
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
            _robotBody = transform.GetChild(0);
            _rigidbody = GetComponent<Rigidbody>();
            _scheduledFlipComponent = GetComponent<ScheduledFlip>();
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
                robotNetworkBridge = GetComponent<RobotNetworkBridge>();
                robotNetworkBridge.enabled = true;
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

            var turtled = CheckUpsideDownAndStuck();
            
            if (_scheduledFlipComponent.enabled && !turtled)
                _scheduledFlipComponent.TryCancelFlip();
            
            else if (!_scheduledFlipComponent.enabled && turtled)
                _scheduledFlipComponent.enabled = true;
        }
        
        private bool CheckUpsideDownAndStuck()
        {
            return _robotBody.up.y < 0 && _rigidbody.angularVelocity.magnitude < MinStuckAngularVelocity;
        }

        private void TryShowBeacons()
        {
            if ((robotNetworkBridge && !robotNetworkBridge.isLocal) 
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
            if (Input.GetKeyDown(KeyCode.Q) 
                && robotIndex == 0 
                && (robotNetworkBridge && robotNetworkBridge.isLocal  || !robotNetworkBridge))
                _showingBeacons = !_showingBeacons;
        }

        public void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterCallbacks(other, this);
        }

    }
}