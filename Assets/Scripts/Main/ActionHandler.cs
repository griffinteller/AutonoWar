using System;
using System.Collections.Generic;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Main
{
    public class ActionHandler : MonoBehaviour
    {
        private Dictionary<string, GameObject>
            _gameObjectsCache; // caching in a hashtable is faster than sequentially searching every time

        private ResetState _resetState = ResetState.Normal;
        private Rigidbody _rigidbody;

        private RobotNetworkBridge _robotNetworkBridge;

        private RobotStateSender _robotStateSender;

        private Vector3
            _startingPosition; // we store these in case we want to reset the robot and we didn't start from 0,0,0

        private Quaternion _startingRotation;

        private WheelCollider[] _wheelColliders;
        public int actorNumber;

        public int internalNegation = 1; // Wheels should keep spinning in same direction regardless of user coord flipping

        public GameObject robotBody;

        // Start is called before the first frame update
        private void Start()
        {
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;

            _wheelColliders = GetComponentsInChildren<WheelCollider>();

            _robotStateSender = GetComponent<RobotStateSender>();

            _gameObjectsCache = new Dictionary<string, GameObject>();

            _robotNetworkBridge = GetComponent<RobotNetworkBridge>();

            _rigidbody = GetComponent<Rigidbody>();

            if (PhotonNetwork.InRoom)
            {
                _robotNetworkBridge.enabled = true;
            }
            else
            {
                GetComponent<UserScriptInterpreter>().enabled = true;
                GetComponent<RobotStateSender>().enabled = true;
            }
        }

        public void Update()
        {

            foreach (var wheelCollider in _wheelColliders)
            {
                
                Debug.Log(wheelCollider.motorTorque);
                Debug.Log(wheelCollider.brakeTorque);
                Debug.Log(wheelCollider.rpm);

            }
            
            switch (_resetState)
            {
                case ResetState.NeedToReset:

                    InternalResetRobot();
                    _resetState = ResetState.NeedToUndoReset;
                    break;

                case ResetState.NeedToUndoReset:

                    UndoReset();
                    _resetState = ResetState.Normal;
                    break;
            }
        }

        private void UndoReset()
        {
            RemoveBrakeForceOnWheelColliders();
            _rigidbody.isKinematic = false;
        }

        private void RemoveBrakeForceOnWheelColliders()
        {
            foreach (var wheelCollider in _wheelColliders) wheelCollider.brakeTorque = 0;
        }

        public void ResetRobot()
        {
            _resetState = ResetState.NeedToReset;
        }

        private void InternalResetRobot()
        {
            transform.position = _startingPosition;
            transform.rotation = _startingRotation;

            robotBody.transform.localPosition = Vector3.zero;
            robotBody.transform.localRotation = Quaternion.identity;
            
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            foreach (var wheelCollider in _wheelColliders)
            {
                wheelCollider.motorTorque = 0;
                wheelCollider.steerAngle = 0;
                wheelCollider.brakeTorque = 10000;
                CachedFind(wheelCollider.name + "Vis").transform.localRotation = Quaternion.identity;
            }
        }

        public GameObject CachedFind(string name)
        {
            GameObject obj;
            try
            {
                obj = _gameObjectsCache[name];
            }
            catch (KeyNotFoundException)
            {
                obj = GameObject.Find(name);
                _gameObjectsCache.Add(name, obj);
            }

            return obj;
        }

        public void SetTireTorque(string tireName, float torque)
        {
            var tireObject = CachedFind(tireName);
            tireObject.GetComponent<WheelCollider>().motorTorque = torque * internalNegation; //* userNegation;
        }

        public void InvertTires()
        {
            foreach (var wheelCollider in _wheelColliders)
            {
                wheelCollider.motorTorque *= -1;
                wheelCollider.steerAngle *= -1;
            }
        }

        public void SetTireSteering(string tireName, float bearing)
        {
            var tireObject = CachedFind(tireName);

            var wheelCollider = tireObject.GetComponent<WheelCollider>();

            wheelCollider.steerAngle = internalNegation * bearing;

            var tireMeshParent = CachedFind(tireName + "Vis");
            
            var currentLocalRight = tireMeshParent.transform.localRotation
             * Vector3.right;

            var currentBearing = Mathf.Rad2Deg * Mathf.Acos(currentLocalRight.x);

            if (currentLocalRight.z > 0)
                currentBearing *= -1;

            tireMeshParent.transform.Rotate(
                robotBody.transform.up, 
                bearing - currentBearing,
                Space.World);


        }

        public void OnDestroy()
        {
            if (PhotonNetwork.InRoom)
            {
                var playerConnection = GameObject.FindWithTag("ConnectionObject").GetComponent<PlayerConnection>();
                playerConnection.robots.Remove(actorNumber);
            }
        }
    }
}