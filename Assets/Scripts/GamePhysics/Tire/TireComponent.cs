using System;
using UnityEngine;

namespace GamePhysics.Tire
{
    [RequireComponent(typeof(TireCollider))]
    [DisallowMultipleComponent]
    public class TireComponent : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Rigidbody      connectedRigidbody;
        [SerializeField] private float          radius;
        [SerializeField] private float          mass                    = 1f;
        [SerializeField] private float          width                   = 1f;
        [SerializeField] private bool           autoConfigureSuspension = true;
        [SerializeField] private float          spring                  = 35000f;
        [SerializeField] private float          damper                  = 4000f;
        [SerializeField] private float          suspensionDistance;
        [SerializeField] private WheelSweepType sweepType = WheelSweepType.Capsule;
        
        [NonSerialized] public TireCollider attachedCollider;
        
        private Quaternion   _startingLocalRotation;


        #endregion

        #region Public Properties

        public float MotorTorque
        {
            get => attachedCollider.MotorTorque;
            set => attachedCollider.MotorTorque = value;
        }

        public float BrakeTorque
        {
            get => attachedCollider.BrakeTorque;
            set => attachedCollider.BrakeTorque = value;
        }

        /*public float SteeringAngle
        {
            get => _collider.SteeringAngle;
            set => _collider.SteeringAngle = value;
        }*/

        public float SteeringAngle => transform.localRotation.eulerAngles.y;
        
        public SingleTireMotor SingleTireMotor { get; set; }

        public float RotationalFriction
        {
            get => attachedCollider.RotationalResistance;
            set => attachedCollider.RotationalResistance = value;
        }

        public float AngularVelocity => attachedCollider.AngularVelocity;
        public float Rpm             => attachedCollider.Rpm;

        #endregion

        #region Event Methods

        public void Awake()
        {
            attachedCollider = GetComponent<TireCollider>();
        }

        public void Start()
        {
            if (!connectedRigidbody)
                connectedRigidbody = GetComponentInParent<Rigidbody>();
            
            attachedCollider.ConnectedRigidbody = connectedRigidbody;

            if (autoConfigureSuspension)
            {
                const float naturalFrequency = 10f;
                const float dampingRatio     = 0.8f;
                
                var numberOfTires = transform.parent.GetComponentsInChildren<TireCollider>().Length;
                var sprungMass    = connectedRigidbody.mass / numberOfTires;
                
                attachedCollider.Spring = sprungMass * naturalFrequency * naturalFrequency;
                attachedCollider.Damper = 2          * dampingRatio     * sprungMass * naturalFrequency;
            }
            else
            {
                attachedCollider.Spring = spring;
                attachedCollider.Damper = damper;
            }
            
            attachedCollider.Mass                 = mass;
            attachedCollider.Length               = suspensionDistance;
            attachedCollider.SweepType            = sweepType;
            attachedCollider.AutoUpdateEnabled    = true;
            attachedCollider.Radius               = radius;
            attachedCollider.RaycastMask          = ~(1 << gameObject.layer); // everything except ourselves
            attachedCollider.Width                = width;

            _startingLocalRotation = transform.localRotation;
        }

        #endregion
        
        #region Other Methods

        public void ResetState()
        {
            if (SingleTireMotor != null)
                SingleTireMotor.Power = 0;

            MotorTorque   = 0;
            BrakeTorque   = 0;
            //SteeringAngle = 0;
            attachedCollider.ResetState();
        }

        public void RotateToBearing(float bearing)
        {
            transform.localRotation = Quaternion.Euler(0, bearing, 0) * _startingLocalRotation;
        }

        #endregion
    }
}