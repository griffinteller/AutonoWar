using System;
using UnityEngine;

namespace Main
{
    public class TireComponent : MonoBehaviour
    {

        //public WheelCollider WheelCollider { get; set; }
        public WheelCollider WheelCollider;
        
        public float bearing;

        public float baseSteerAngle;
        

        public void ResetTireSteering()
        {
            bearing = 0;
        }

        public void Update()
        {
            WheelCollider.steerAngle = bearing + baseSteerAngle;
        }
    }
}