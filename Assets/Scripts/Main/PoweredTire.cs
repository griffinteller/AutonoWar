using System;
using UnityEngine;

namespace Main
{
    public class PoweredTire : MonoBehaviour
    {

        public Vector3 relativeConnectionPosition;
        public float radius;

        [SerializeField] private float touchDistance;
        [SerializeField] private int samples; // must be even

        private float _motorTorque;
        private float _brakeTorque;
        private float _steerBearing;
        private float _rpm;

        private Vector3 _centerOfMass;

        private readonly LayerMask _layers = LayerMask.GetMask("LidarVisible", "Robots");

        public float MotorTorque
        {
            get => _motorTorque; 
            set => _motorTorque = value;
        }

        public float BrakeTorque
        {
            get => _brakeTorque; 
            set => _brakeTorque = value;
        }

        public float SteerBearing
        {
            get => _steerBearing;
            set => _steerBearing = value;
        }

        public float Rpm
        {
            get => _rpm;
        }

        private Rigidbody _rootRigidbody;

        public void Start()
        {

            _rootRigidbody = GetRootRigidbody();

        }

        private Rigidbody GetRootRigidbody()
        {
            
            var rootTransform = _rootRigidbody;
            return rootTransform.GetComponent<Rigidbody>();

        }

        public void FixedUpdate()
        {

            var contacts = GetContacts();

            foreach (var contact in contacts)
            {
                
                ApplyForce(contact);
                
            }

        }

        private RaycastHit[] GetContacts()
        {
            
            var contacts = new RaycastHit[samples];

            var t = transform;
            var sampleDegreesApart = 360f / samples;
            
            for (var sample = 0; sample < samples; sample++)
            {

                var rayDirection = Quaternion.AngleAxis(sampleDegreesApart, t.right) * -t.up;
                Physics.Raycast(
                    t.position, rayDirection, 
                    out contacts[sample], 
                    radius + touchDistance,
                    _layers);

            }

            return contacts;

        }

        private void ApplyForce(RaycastHit contact)
        {

            var rightVector = transform.right;

            var projectedNormal = Vector3.ProjectOnPlane(contact.normal, rightVector).normalized;
            var forceDirection = Vector3.Cross(rightVector, projectedNormal);
            var forceVector = forceDirection * MotorTorque / radius;

            _rootRigidbody.AddForceAtPosition(
                forceVector, 
                transform.TransformPoint(relativeConnectionPosition),
                ForceMode.Force);

        }
        
    }
}