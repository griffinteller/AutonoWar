using UnityEngine;

namespace Utility
{
    public struct PositionRotationPair
    {
        public Vector3 position;
        public Quaternion rotation;

        public PositionRotationPair(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}