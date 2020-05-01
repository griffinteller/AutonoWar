using UnityEngine;

namespace Main.Building
{
    public class ConnectionDescription
    {
        
        public Vector3 connectionCenter;
        public Vector3 outwardsDirection;

        public static implicit operator bool(ConnectionDescription a)
        {

            return a != null;

        }

        public ConnectionDescription(Vector3 connectionCenter, Vector3 outwardsDirection)
        {

            this.connectionCenter = connectionCenter;
            this.outwardsDirection = outwardsDirection;

        }

    }
}