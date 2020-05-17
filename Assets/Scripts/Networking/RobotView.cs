using System;
using Main;
using Photon.Pun;
using UnityEngine;

namespace Networking
{
    public class RobotView : MonoBehaviour, IPunObservable
    {

        public WheelCollider[] wheelColliderParent;
        public Transform robotBody;

        private Rigidbody _rigidbody;
        private TireComponent[] _tireComponents;

        private Vector3 _desiredPosition;
        private Quaternion _desiredRotation;
        private Vector3 _desiredVelocity;

        public void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public NetworkedRobotState GetRobotState()
        {

            if (_tireComponents == null || _tireComponents.Length == 0)
                _tireComponents = GetComponentsInChildren<TireComponent>();
            
            var bearings = new float[_tireComponents.Length];
            var rpms = new float[_tireComponents.Length];
            for (var i = 0; i < _tireComponents.Length; i++)
            {
                bearings[i] = _tireComponents[i].bearing;
                rpms[i] = _tireComponents[i].WheelCollider.rpm;
            }

            return new NetworkedRobotState
            {
                position = transform.position,
                rotation = robotBody.rotation,
                tireBearings = bearings,
                tireRpms = rpms,
                velocity = _rigidbody.velocity
            };
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
                stream.SendNext(GetRobotState());

            else
            {
                var remoteState = (NetworkedRobotState) stream.ReceiveNext();
                SyncWithRemoteState(remoteState);
            }
        }

        private void SyncWithRemoteState(NetworkedRobotState remoteState)
        {

            _desiredPosition = remoteState.position;
            _desiredRotation = remoteState.rotation;
            _desiredVelocity = remoteState.velocity;
            

        }
    }
}