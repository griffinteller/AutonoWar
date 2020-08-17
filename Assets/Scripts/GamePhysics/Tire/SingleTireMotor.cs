using System;
using UnityEngine;

namespace GamePhysics.Tire
{
    public class SingleTireMotor : MonoBehaviour
    {
        #region Public Fields
        
        [Tooltip("If the attached collider is on the same GameObject, it will be automatically added.")]
        public TireComponent attachedTireComponent;
        
        #endregion

        #region Private Fields

        [SerializeField] private float maxPower           = 5000f;
        [SerializeField] private float maxMotorTorque     = 2000f;
        [SerializeField] private float maxAngularVelocity = 100f;
        [SerializeField] private float maxBrakeTorque     = 500f;
        [SerializeField] private float rotationalFriction = 0.05f;

        private float _power;

        #endregion
        
        #region Public Properties

        public float MaxPower
        {
            get => maxPower;
            set => maxPower = Mathf.Max(0, value);
        }

        public float MaxMotorTorque
        {
            get => maxMotorTorque;
            set => maxMotorTorque = Mathf.Max(0, value);
        }

        public float MaxAngularVelocity
        {
            get => maxAngularVelocity;
            set => maxAngularVelocity = Mathf.Max(0,value);
        }

        public float MaxRpm
        {
            get => maxAngularVelocity * 9.55414012739f;
            set => maxAngularVelocity = Mathf.Max(0,value) / 9.55414012739f;
        }

        public float RotationalFriction
        {
            get => attachedTireComponent.RotationalFriction;
            set => attachedTireComponent.RotationalFriction = Mathf.Max(0, value);
        }

        public float MotorTorque
        {
            get => attachedTireComponent.MotorTorque;
            private set
            {
                if (Mathf.Abs(attachedTireComponent.AngularVelocity) > maxAngularVelocity)
                    value = 0;
                
                attachedTireComponent.MotorTorque = Mathf.Sign(value)
                                                  * Mathf.Min(maxMotorTorque, Mathf.Abs(value));
            } 
        }

        public float BrakeTorque
        {
            get => attachedTireComponent.BrakeTorque;
            set => attachedTireComponent.BrakeTorque = Mathf.Clamp(value, 0, maxBrakeTorque);
        }

        public float Power
        {
            get => _power;
            set
            {
                if (value == 0)
                {
                    _power      = 0;
                    MotorTorque = 0;
                    return;
                }

                _power = Mathf.Sign(value)
                       * Mathf.Min(maxPower, Mathf.Abs(value));

                var angularVelocity = attachedTireComponent.AngularVelocity;

                if (angularVelocity == 0)
                    MotorTorque = Mathf.Sign(_power) * maxMotorTorque;
                else
                    MotorTorque = _power / Mathf.Abs(angularVelocity); // gets clamped anyways, no need to do it twice
            }
        }

        #endregion

        #region Event Methods

        public void Start()
        {
            if (attachedTireComponent == null)
            {
                attachedTireComponent = GetComponent<TireComponent>();

                if (attachedTireComponent == null)
                {
                    Destroy(this);
                    throw new Exception("No tire component is set, and gameobject does not have one!");
                }
            }

            attachedTireComponent.SingleTireMotor = this;

            RotationalFriction = rotationalFriction;
        }

        public void FixedUpdate()
        {
            Power = _power;
        }

        #endregion
    }
}
