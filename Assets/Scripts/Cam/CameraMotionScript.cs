using System;
using GameDirection;
using Networking;
using UnityEngine;
using Utility;

namespace Cam
{
    public class CameraMotionScript : MonoBehaviour
    {
        private float _bearingDiff;
        private Vector3 _currentDelta;
        private float _currentRobotBearing;
        private Vector3 _desiredDelta;
        private bool _smooth;
        private Vector3 _velocity;
        private Bounds _mapBounds;

        public CameraMode cameraMode;

        public Transform centerObject;
        public float groundBuffer = 0.05f;
        public const float SideBuffer = 10f;

        public bool interactable = true;
        public float lookSensitivity = 50;
        public float maxCameraAngle = 80;
        public float maxDistance = 500f;
        public float maxSmoothSpeed = 10f;
        public float minDistance = 3f;
        public PlayerConnection playerConnection;
        public float smoothTime = 0.5f;
        public Vector3 startingDisplacement;

        public Vector3 viewCenter;

        public float zoomSensitivity = 50;

        private void Start()
        {
            zoomSensitivity /= 17;
            lookSensitivity /= 6;

            try
            {
                viewCenter = playerConnection.startingPositionAndRotation.position;
            }
            catch (NullReferenceException)
            {
                viewCenter = centerObject.position;
            }

            transform.position = viewCenter + startingDisplacement;
            _currentDelta = transform.position - viewCenter;
            _desiredDelta = _currentDelta;
            
            _mapBounds = new Bounds();
            _mapBounds.size = MapEnumWrapper.MapSizes[FindObjectOfType<GameDirector>().CurrentMap]
                - Vector3.one * SideBuffer;

            print(_mapBounds);
            
            transform.LookAt(viewCenter);
        }

        // Update is called once per frame
        public void Update()
        {
            KeyCheck();

            if (!interactable)
                return;

            if (Input.GetKey(KeyCode.LeftShift))
                return;

            if (centerObject) viewCenter = centerObject.position;
            
            transform.position = _currentDelta + viewCenter;
            _currentRobotBearing = GetRobotBearing();

            _desiredDelta = ZoomDelta(_desiredDelta);
            var finalMagnitude = _desiredDelta.magnitude;

            if (cameraMode == CameraMode.Chase)
            {
                _desiredDelta = TrackRobotRotationYaw(_desiredDelta);
                _smooth = true;
            }
            else
            {
                _smooth = false;
            }

            _desiredDelta = RotateDeltaFromMouse(_desiredDelta);
            _desiredDelta *= finalMagnitude / _desiredDelta.magnitude;
            _desiredDelta = KeepDeltaInBounds(_desiredDelta);
            _desiredDelta = RaiseDeltaAboveTerrain(_desiredDelta);

            if (_smooth)
                _currentDelta = Vector3.SmoothDamp(
                    _currentDelta, _desiredDelta,
                    ref _velocity,
                    smoothTime, maxSmoothSpeed * _currentDelta.magnitude);
            else
                _currentDelta = _desiredDelta;

            if (centerObject) _bearingDiff = _currentRobotBearing - GetDeltaBearing(_desiredDelta);

            transform.LookAt(viewCenter);
        }

        private Vector3 KeepDeltaInBounds(Vector3 delta)
        {
            var pos = centerObject.transform.position + delta;
            if (!_mapBounds.Contains(pos))
            {
                var result = _mapBounds.ClosestPoint(pos);
                print("point: " + result);
                return _mapBounds.ClosestPoint(pos) - centerObject.position;
            }
            
            return delta;
        }

        private Vector3 TrackRobotRotationYaw(Vector3 delta)
        {
            var currentDeltaBearing = GetDeltaBearing(delta);
            return Quaternion.Euler(0, _currentRobotBearing - currentDeltaBearing - _bearingDiff, 0) * delta;
        }

        private float GetRobotBearing()
        {
            var projectedForwardVector = Vector3.ProjectOnPlane(
                centerObject.forward, Vector3.up).normalized;
            var bearing = Mathf.Rad2Deg * Mathf.Acos(projectedForwardVector.z);
            if (projectedForwardVector.x < 0)
                bearing *= -1;
            return bearing;
        }

        private float GetDeltaBearing(Vector3 delta)
        {
            var projectedForwardVector = Vector3.ProjectOnPlane(
                delta, Vector3.up).normalized;
            var bearing = Mathf.Rad2Deg * Mathf.Acos(projectedForwardVector.z);
            if (projectedForwardVector.x < 0)
                bearing *= -1;
            return bearing;
        }

        private Vector3 RotateDeltaFromMouse(Vector3 delta)
        {
            if (Input.GetMouseButton(0))
            {
                var mouseDelta = InputUtility.GetMouseDelta();
                var horizRotation = Quaternion.Euler(0, mouseDelta.x * lookSensitivity, 0);
                var vertRotation = Quaternion.AngleAxis(-mouseDelta.y * lookSensitivity, transform.right);

                var rotDelta = vertRotation * horizRotation * delta;
                if (IsDeltaBelowTerrain(rotDelta) || IsDeltaAngleTooHigh(rotDelta)) return delta;

                return rotDelta;
            }

            return delta;
        }

        private Vector3 ZoomDelta(Vector3 delta)
        {
            var scrollDelta = -Input.GetAxis("Mouse ScrollWheel");
            var zoomedDelta = delta * (float) Math.Pow(1 + zoomSensitivity, scrollDelta);

            // if we are too close
            var newDistance = zoomedDelta.magnitude;
            if (newDistance < minDistance)
                zoomedDelta *= minDistance / newDistance;
            else if (newDistance > maxDistance) zoomedDelta *= maxDistance / newDistance;

            return zoomedDelta;
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
            if (pos.y < terrainHeight + groundBuffer) pos.y = terrainHeight + groundBuffer;

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

        public void KeyCheck()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                cameraMode = (CameraMode)
                    (((int) cameraMode + 1) % Enum.GetNames(typeof(CameraMode)).Length);
                Debug.Log("New camera mode: " + cameraMode);
            }
        }
    }
}