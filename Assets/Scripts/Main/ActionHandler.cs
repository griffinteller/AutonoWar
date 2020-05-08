using System;
using System.Collections.Generic;
using System.IO.Pipes;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Main
{
    public class ActionHandler : MonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _gameObjectsCache = new Dictionary<string, GameObject>(); 
        // caching in a hashtable is faster than sequentially searching every time

        private ResetState _resetState = ResetState.Normal;
        private Rigidbody _rigidbody;

        private RobotNetworkBridge _robotNetworkBridge;

        private Vector3
            _startingPosition; // we store these in case we want to reset the robot and we didn't start from 0,0,0

        private Quaternion _startingRotation;
        
        public int actorNumber;

        public int internalNegation = 1; // Wheels should keep spinning in same direction regardless of user coord flipping

        public GameObject robotBody;

        private readonly Dictionary<string, TireComponent> _tireComponents = new Dictionary<string, TireComponent>();

        // Start is called before the first frame update
        private void Start()
        {

            _startingPosition = transform.position;
            _startingRotation = transform.rotation;
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

        public void LoadTiresIntoDict()
        {
            
            foreach (var tireComponent in transform.GetComponentsInChildren<TireComponent>())
            {
                _tireComponents.Add(tireComponent.name, tireComponent);
            }
            
        }

        public void Update()
        {

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
            RemoveBrakeForceOnTires();
            _rigidbody.isKinematic = false;
        }

        private void RemoveBrakeForceOnTires()
        {
            foreach (var tire in _tireComponents) 
                tire.Value.WheelCollider.brakeTorque = 0;
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

            foreach (var tire in _tireComponents.Values)
            {
                tire.WheelCollider.motorTorque = 0;
                tire.bearing = 0;
                tire.WheelCollider.brakeTorque = 10000;
            }
        }

        public void SetTireTorque(string tireName, float torque)
        {

            var tireObject = _tireComponents[tireName + "Vis"];
            tireObject.WheelCollider.motorTorque = torque * internalNegation; //* userNegation;
            
        }

        public void InvertTires()
        {
            foreach (var tire in _tireComponents.Values)
            {
                tire.WheelCollider.motorTorque *= -1;
                tire.bearing *= -1;
            }
        }

        public void SetTireSteering(string tireName, float bearing)
        {

            var tireComponent = _tireComponents[tireName + "Vis"];

            tireComponent.bearing = internalNegation * bearing;
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