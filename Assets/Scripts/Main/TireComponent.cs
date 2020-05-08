using System;
using UnityEngine;

namespace Main
{
    public class TireComponent : MonoBehaviour
    {

        public WheelCollider WheelCollider { get; set; }
        
        public float bearing;

        public float baseSteerAngle;

        public void Update()
        {

            WheelCollider.steerAngle = bearing + baseSteerAngle;

        }
    }
}