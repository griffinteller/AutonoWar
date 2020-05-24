using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class ActionHandler : MonoBehaviour
    {
        private readonly Dictionary<string, TireComponent> _tireComponents =
            new Dictionary<string, TireComponent>();

        private ResetState _resetState = ResetState.Normal;
        private bool _resetting;
        private Rigidbody _rigidbody;

        // we store these in case we want to reset the robot and we didn't start from 0,0,0
        private Vector3 _startingPosition;
        private Quaternion _startingRotation;
        [SerializeField] private GameObject robotBody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            LoadStartingPosition();
        }

        private void LoadStartingPosition()
        {
            var t = transform;

            _startingPosition = t.position;
            _startingRotation = t.rotation;
        }

        public void LoadTiresIntoDict()
        {
            foreach (var tireComponent in transform.GetComponentsInChildren<TireComponent>())
                _tireComponents.Add(tireComponent.name, tireComponent);
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

                    if (!_resetting)
                    {
                        _resetting = true;
                        break;
                    }

                    _resetting = false;
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
                tire.WheelCollider.brakeTorque = 10000000000000000;
                tire.ResetTireSteering();
            }
        }

        public void SetTireTorque(string tireName, float torque)
        {
            var tireObject = _tireComponents[tireName + "Vis"];
            tireObject.WheelCollider.motorTorque = torque;
        }

        public void SetTireSteering(string tireName, float bearing)
        {
            var tireComponent = _tireComponents[tireName + "Vis"];

            tireComponent.bearing = bearing;
        }
    }
}