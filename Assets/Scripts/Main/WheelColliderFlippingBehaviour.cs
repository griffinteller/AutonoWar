using UnityEngine;
using Utility;

namespace Main
{
    public class WheelColliderFlippingBehaviour : MonoBehaviour
    {
        private ActionHandler _actionHandler;
        private bool _flipped;

        private Transform _robotBody;

        public void Start()
        {
            _actionHandler = GetComponent<ActionHandler>();
            _robotBody = transform.Find("Body");
        }

        public void Update()
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

            transform.rotation = upVectorCorrection * innerRotation;
            robotBodyTransform.rotation = innerRotation; // fixes inner rotation because it gets pulled along with its parent

            if (robotBodyTransform.up.y < 0)
            {
                _actionHandler.internalNegation = -1;

                if (!_flipped)
                {
                    _actionHandler.InvertTires();
                    _flipped = true;
                }
            }
            else
            {
                _actionHandler.internalNegation = 1;
                _flipped = false;
            }
        }
    }
}