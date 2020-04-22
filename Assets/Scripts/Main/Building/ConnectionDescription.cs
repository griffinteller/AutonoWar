using UnityEngine;

namespace Main.Building
{
    public class ConnectionDescription
    {
        
        public Vector3 newObjectCenter;
        public Vector3 outwardsDirection;

        public static implicit operator bool(ConnectionDescription a)
        {

            return a != null;

        }

        public ConnectionDescription(Vector3 newObjectCenter, Vector3 outwardsDirection)
        {

            this.newObjectCenter = newObjectCenter;
            this.outwardsDirection = outwardsDirection;

        }

    }
}