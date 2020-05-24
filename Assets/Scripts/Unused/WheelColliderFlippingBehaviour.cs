using UnityEngine;

namespace Unused
{
    public class WheelColliderFlippingBehaviour : MonoBehaviour
    {
        /*private ActionHandler _actionHandler;
        private bool _flippedLastFrame;

        private Transform _robotBody;

        public void Start()
        {
            _actionHandler = GetComponent<ActionHandler>();
            _robotBody = transform.Find("Body");
        }

        public void LateUpdate()
        {
            // Up vector of robot parent should be straight up with forward coplanar with inner forward

            var terrainNormal = TerrainUtility.GetTerrainNormal(transform.position);
            var robotBodyTransform = _robotBody.transform; // caching is slightly faster
            var robotBodyRightVector = robotBodyTransform.right;

            var upVectorCorrectionMagnitude = Vector3.SignedAngle(
                robotBodyTransform.up, terrainNormal, robotBodyRightVector);
            
            var upVectorCorrection = Quaternion.AngleAxis(
                upVectorCorrectionMagnitude, robotBodyRightVector);
            
            var innerRotation = robotBodyTransform.rotation;
            var outerRotation = upVectorCorrection * innerRotation;

            if (Vector3.Angle(robotBodyTransform.forward, outerRotation * Vector3.forward) > 90)
                outerRotation = Quaternion.AngleAxis(180, outerRotation * Vector3.up) * outerRotation;

            transform.rotation = outerRotation;
            robotBodyTransform.rotation = innerRotation; // fixes inner rotation because it gets pulled along with its parent
            var flippedCurrently = robotBodyTransform.up.y < 0;

            if (flippedCurrently != _flippedLastFrame)
            {
                if (flippedCurrently)
                    _actionHandler.internalNegation = -1;
                else
                    _actionHandler.internalNegation = 1;
                
                _actionHandler.AdjustTireOrientation();
                _flippedLastFrame = flippedCurrently;
            }
            
        }*/
    }
}