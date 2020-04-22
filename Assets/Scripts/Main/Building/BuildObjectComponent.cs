using System;
using UnityEngine;

namespace Main.Building
{
    public class BuildObjectComponent : MonoBehaviour
    {

        public BuildObjectShape shape;

        public bool removable = true;

        [HideInInspector] public Vector3 connectingFaceOutwardsDirection;

        public void Start()
        {

            connectingFaceOutwardsDirection = GetConnectingFaceOutwardsDirection();

        }

        public ConnectionDescription GetConnection(Vector3 hitpointWorldspace)
        {

            var hitpointLocalSpace = transform.InverseTransformPoint(hitpointWorldspace);
            
            var distanceToNewCenter = new Vector3();
            switch (shape)
            {
                
                
                case BuildObjectShape.Cube:

                    var deadzone = 0.025f;
                    var sideLength = 0.5f;

                    if (Math.Abs(hitpointLocalSpace.x) - (sideLength / 2 - deadzone) > 0)
                    {
                        distanceToNewCenter.x = Mathf.Sign(hitpointLocalSpace.x) * sideLength;
                    }
                    
                    if (Math.Abs(hitpointLocalSpace.y) - (sideLength / 2 - deadzone) > 0)
                    {
                        distanceToNewCenter.y = Mathf.Sign(hitpointLocalSpace.y) * sideLength;
                    }
                    
                    if (Math.Abs(hitpointLocalSpace.z) - (sideLength / 2 - deadzone) > 0)
                    {
                        distanceToNewCenter.z = Mathf.Sign(hitpointLocalSpace.z) * sideLength;
                    }

                    if (distanceToNewCenter.magnitude - sideLength > float.Epsilon)
                    {
                        return null; // don't connect if we're on a corner
                    }
                    
                    break;
                
            }

            var worldSpaceNewCenter = transform.TransformPoint(distanceToNewCenter);
            var worldSpaceOutwardsDirection = (worldSpaceNewCenter - transform.position).normalized;
            return new ConnectionDescription(worldSpaceNewCenter, worldSpaceOutwardsDirection);

        }

        private Vector3 GetConnectingFaceOutwardsDirection()
        {
            
            switch (shape)
            {
                
                
                case BuildObjectShape.Cube:

                    return Vector3.down;

            }
            
            throw new NotImplementedException();
            
        }
        
    }
}