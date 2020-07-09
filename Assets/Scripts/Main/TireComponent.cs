using UnityEngine;

namespace Main
{
    public class TireComponent : MonoBehaviour
    {
        public float baseSteerAngle;

        public float bearing;

        //public WheelCollider WheelCollider { get; set; }
        public WheelCollider WheelCollider;


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