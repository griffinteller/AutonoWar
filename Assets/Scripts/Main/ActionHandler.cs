using System.Collections.Generic;
using GamePhysics.Tire;
using UnityEngine;

namespace Main
{
    public class ActionHandler : MonoBehaviour
    {
        public readonly Dictionary<string, TireComponent> TireComponents =
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
                TireComponents.Add(tireComponent.name, tireComponent);
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
            _rigidbody.isKinematic = false;
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

            foreach (var tire in TireComponents.Values)
            {
                tire.ResetState();
            }
        }

        public void SetTirePower(string tireName, float power)
        {
            var tireObject = TireComponents[tireName];
            tireObject.SingleTireMotor.Power = power;
        }

        public void SetTireSteering(string tireName, float bearing)
        {
            var tireComponent = TireComponents[tireName];

            tireComponent.RotateToBearing(bearing);
        }

        public void SetTireBrake(string tireName, float torque)
        {
            var tireComponent = TireComponents[tireName];
            tireComponent.BrakeTorque = torque;
        }
    }
}