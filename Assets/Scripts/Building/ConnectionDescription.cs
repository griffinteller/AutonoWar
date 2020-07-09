using UnityEngine;

namespace Building
{
    public class ConnectionDescription
    {
        public Vector3 connectionCenter;
        public Vector3 outwardsDirection;

        public ConnectionDescription(Vector3 connectionCenter, Vector3 outwardsDirection)
        {
            this.connectionCenter = connectionCenter;
            this.outwardsDirection = outwardsDirection;
        }

        public static implicit operator bool(ConnectionDescription a)
        {
            return a != null;
        }
    }
}