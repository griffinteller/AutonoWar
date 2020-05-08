using System;
using UnityEngine;

namespace Main
{
    
    public class TireMeshAnimator : MonoBehaviour
    {
        
        private const float LerpTimePerMeter = 0.5f;
        private const float LerpTimePerRotation = 1f;
        
        [SerializeField] private Transform innerMesh;
        
        [HideInInspector] public Transform tireMeshRoot;
        [HideInInspector] public WheelCollider wheelCollider;

        private Vector3 _desiredLocalPosition;  // to facilitate lerping
        private Quaternion _desiredLocalRotation;

        public void Start()
        {
            
            RotateInnerTireFromRpm();

            SyncDesiredRotationWithSteerAngle();
            SyncDesiredPositionWithColliderCenter();
            
        }

        public void Update()
        {
            
            LerpState();

            RotateInnerTireFromRpm();

            SyncDesiredRotationWithSteerAngle();
            SyncDesiredPositionWithColliderCenter();

        }

        private void RotateInnerTireFromRpm()
        {

            var rpm = wheelCollider.rpm;

            innerMesh.Rotate(rpm * 360 / 60 * Time.deltaTime, 0, 0);

        }

        private void SyncDesiredRotationWithSteerAngle()
        {
            
            _desiredLocalRotation = Quaternion.Euler(0, wheelCollider.steerAngle, 0);

        }

        private void SyncDesiredPositionWithColliderCenter()
        {
            
            wheelCollider.GetWorldPose(out var pos, out _);
            _desiredLocalPosition = tireMeshRoot.InverseTransformPoint(pos);

        }

        private void LerpState()
        {

            var t = transform;

            var deltaDistance = (_desiredLocalPosition - t.localPosition).magnitude;

            t.localPosition = Vector3.Lerp(
                t.localPosition, _desiredLocalPosition,
                Time.deltaTime / (deltaDistance * LerpTimePerMeter));

            var deltaDegress = Quaternion.Angle(t.localRotation, _desiredLocalRotation);
            
            t.localRotation = Quaternion.Slerp(
                t.localRotation, _desiredLocalRotation,
                Time.deltaTime / (deltaDegress / 360 * LerpTimePerRotation));

        }
        
    }
}