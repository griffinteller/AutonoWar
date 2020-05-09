using System;
using UnityEngine;

namespace Main
{
    public class TireComponent : MonoBehaviour
    {

        public WheelCollider WheelCollider { get; set; }
        
        public float bearing;

        public float baseSteerAngle;

        public float originalSteerAngle;

        public void Start()
        {

            originalSteerAngle = baseSteerAngle;

        }

        public void ResetTireSteering()
        {

            baseSteerAngle = originalSteerAngle;
            bearing = 0;

        }

        public void Update()
        {

            WheelCollider.steerAngle = bearing + baseSteerAngle;

        }
    }
}