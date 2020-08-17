using System;
using System.Linq;
using System.Runtime.InteropServices;
using GamePhysics.Tire;
using Main;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class Tires : ISensor
    {
        public TireDescription[] tires;
        
        private readonly TireComponent[] _tireComponents;

        [Serializable]
        public class TireDescription
        {
            public string name;
            public float  motorPower;
            public float  motorTorque;
            public float  brakeTorque;
            public float  rpm;
            public float  suspensionExtension;
            public bool   isGrounded;
            public float  springForce;
            public float  longitudinalSlip;
            public float  lateralSlip;
        }

        public Tires(GameObject robotBody)
        {
            _tireComponents = robotBody.GetComponentsInChildren<TireComponent>();

            tires = new TireDescription[_tireComponents.Length];
            
            Update();
        }

        public void Update()
        {
            for (var i = 0; i < _tireComponents.Length; i++)
            {
                var component = _tireComponents[i];
                var collider  = component.attachedCollider;
                //Debug.Log(tires);
                tires[i] = new TireDescription
                {
                    name                = component.name,
                    motorPower          = component.SingleTireMotor.Power,
                    motorTorque         = component.MotorTorque,
                    brakeTorque         = component.BrakeTorque,
                    rpm                 = component.Rpm,
                    suspensionExtension = collider.Length - collider.CompressionDistance,
                    isGrounded          = collider.IsGrounded,
                    springForce         = collider.SpringForce,
                    lateralSlip         = collider.LateralSlip,
                    longitudinalSlip    = collider.LongitudinalSlip
                };
            }
        }
    }
}
