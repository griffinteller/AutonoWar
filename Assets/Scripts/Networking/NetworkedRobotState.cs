using System;
using UnityEngine;

namespace Networking
{
    [Serializable]
    public class NetworkedRobotState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float[] tireBearings;
        public float[] tireRpms;
        public Vector3 velocity;
    }
}