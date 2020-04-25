using System;
using UnityEngine;

namespace Main.Building
{
    public class BuildCubeComponent : BuildObjectComponent
    {
        public override ConnectionDescription GetConnection(Vector3 hitPointWorldSpace)
        {
            
            var hitPointLocalSpace = transform.InverseTransformPoint(hitPointWorldSpace);
            var distanceToNewCenter = new Vector3();
            
            const float deadZone = 0.025f;
            const float sideLength = 0.5f;

            if (Math.Abs(hitPointLocalSpace.x) - (sideLength / 2 - deadZone) > 0)
            {
                distanceToNewCenter.x = Mathf.Sign(hitPointLocalSpace.x) * sideLength;
            }
            
            if (Math.Abs(hitPointLocalSpace.y) - (sideLength / 2 - deadZone) > 0)
            {
                distanceToNewCenter.y = Mathf.Sign(hitPointLocalSpace.y) * sideLength;
            }
            
            if (Math.Abs(hitPointLocalSpace.z) - (sideLength / 2 - deadZone) > 0)
            {
                distanceToNewCenter.z = Mathf.Sign(hitPointLocalSpace.z) * sideLength;
            }

            if (distanceToNewCenter.magnitude - sideLength > float.Epsilon)
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
    }
}