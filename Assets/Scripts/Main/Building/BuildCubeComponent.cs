using System;
using UnityEngine;

namespace Main.Building
{
    public class BuildCubeComponent : BuildObjectComponent
    {

        public float radius;
        public float deadZone = 0.025f;

        public override ConnectionDescription GetConnection(Vector3 hitPointWorldSpace)
        {
            
            var hitPointLocalSpace = transform.InverseTransformPoint(hitPointWorldSpace);
            var distanceToNewCenter = new Vector3();

            if (Math.Abs(hitPointLocalSpace.x) - (radius - deadZone) > 0)
            {
                distanceToNewCenter.x = Mathf.Sign(hitPointLocalSpace.x) * radius;
            }
            
            if (Math.Abs(hitPointLocalSpace.y) - (radius - deadZone) > 0)
            {
                distanceToNewCenter.y = Mathf.Sign(hitPointLocalSpace.y) * radius;
            }
            
            if (Math.Abs(hitPointLocalSpace.z) - (radius - deadZone) > 0)
            {
                distanceToNewCenter.z = Mathf.Sign(hitPointLocalSpace.z) * radius;
            }

            if (distanceToNewCenter.magnitude - radius > float.Epsilon)
            {
                return null; // don't connect if we're on a corner
            }

            var transformCached = transform;
            var worldSpaceNewCenter = transformCached.TransformPoint(distanceToNewCenter);
            var worldSpaceOutwardsDirection = (worldSpaceNewCenter - transformCached.position).normalized;
            return new ConnectionDescription(worldSpaceNewCenter, worldSpaceOutwardsDirection);

        }

        public override Vector3 GetConnectingFaceOutwardsDirection()
        {
            
            return Vector3.down;
            
        }

        public override float GetRadius()
        {

            return radius;

        }
    }
}