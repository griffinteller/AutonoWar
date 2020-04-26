using System;
using Networking;
using UnityEngine;
using Utility;

namespace Main
{
    public class CameraMotionScript : MonoBehaviour
    {

        public bool interactable = true;

        public Transform centerObject;
        public PlayerConnection playerConnection;

        public Vector3 viewCenter;
        public Vector3 startingDisplacement;

        public float zoomSensitivity;
        public float lookSensitivity;
        public float minDistance = 0.5f;
        public float maxDistance = 10f;
        public float groundBuffer = 0.5f;
        public float maxCameraAngle = 80;

        private Vector3 _centerDelta;

        private Vector2 _mousePosLastFrame;

        private void Start()
        {

            zoomSensitivity /= 17;
            lookSensitivity /= 6;

            try
            {

                viewCenter = playerConnection.startingPosition;

            }
            catch (NullReferenceException)
            {
                
                viewCenter = centerObject.position;
                
            }
            transform.position = viewCenter + startingDisplacement;
            _centerDelta = transform.position - viewCenter;
            transform.LookAt(viewCenter);

        }

        // Update is called once per frame
        void Update()
        {

            if (!interactable) 
                return;

            if (Input.GetKey(KeyCode.LeftShift)) 
                return;

            if (centerObject)
            {

                viewCenter = centerObject.position;

            }

            transform.position = _centerDelta + viewCenter;
            var scrollDelta = -Input.GetAxis("Mouse ScrollWheel");
            var zoomedDelta = _centerDelta * (float) Math.Pow(1 + zoomSensitivity, scrollDelta);
            
            // if we are too close
            var newDistance = zoomedDelta.magnitude;
            if (newDistance < minDistance)
            {

                zoomedDelta *= minDistance / newDistance;

            }
            else if (newDistance > maxDistance)
            {
                
                zoomedDelta *= maxDistance / newDistance;
                
            }

            zoomedDelta = RaiseDeltaAboveTerrain(zoomedDelta);
            var rotDelta = zoomedDelta;
            if (Input.GetMouseButton(0))
            {

                var mouseDelta = InputUtility.GetMouseDelta();
                var horizRotation = Quaternion.Euler(0, mouseDelta.x * lookSensitivity, 0);
                var vertRotation = Quaternion.AngleAxis(-mouseDelta.y * lookSensitivity, transform.right);
            
                rotDelta = vertRotation * horizRotation * rotDelta;
                if (IsDeltaBelowTerrain(rotDelta) || IsDeltaAngleTooHigh(rotDelta))
                {

                    rotDelta = zoomedDelta;

                }
                
            }
            
            transform.position = viewCenter + rotDelta;
            _centerDelta = rotDelta;
            transform.LookAt(viewCenter);

        }

        private bool IsDeltaAngleTooHigh(Vector3 delta)
        {

            delta = delta.normalized;
            var cameraAngle = Mathf.Rad2Deg * Mathf.Asin(delta.y);
            return cameraAngle > maxCameraAngle;

        }

        private Vector3 RaiseDeltaAboveTerrain(Vector3 delta)
        {
            
            var pos = viewCenter + delta;
            var terrainHeight = TerrainUtility.GetClosestCurrentTerrain(transform.position).SampleHeight(pos);
            if (pos.y < terrainHeight + groundBuffer)
            {

                pos.y = terrainHeight + groundBuffer;

            }

            return pos - viewCenter;
            
        }

        private bool IsDeltaBelowTerrain(Vector3 delta)
        {

            return (viewCenter + delta).y < TerrainUtility.GetClosestCurrentTerrain(transform.position)
                .SampleHeight(viewCenter + delta) + groundBuffer;

        }

        public void SetCenterObject(GameObject obj)
        {

            centerObject = obj.transform;

        }

    }

}