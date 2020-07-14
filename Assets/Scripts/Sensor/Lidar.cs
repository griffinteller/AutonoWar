using System;
using Main;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utility;

namespace Sensor
{
    [Serializable]
    public class Lidar : ISensor
    {
        private GameObject _robotBody;
        private NativeArray<RaycastHit> _raycastHits;
        private NativeArray<RaycastCommand> _commands;
        private JobHandle _raycastsHandle;

        private int _matrixHeight;
        private int _matrixWidth;
        private int _totalRaycasts;

        private readonly NativeArray<Vector3> _localRaycastDirections;
        private readonly float[] _distancesPlaceholder;
        private readonly RaycastHit[] _hitsPlaceholder;
        private readonly LayerMask _mask;
        
        public FloatArrayContainer[] distanceMatrix;
        
        public readonly float horizontalDensity = 5;
        public readonly float verticalDensity = 5;
        public readonly float range = 300;
        public readonly float[] verticalFOVBounds  = { -30, 30}; // how many degrees down, how many up

        public Lidar(GameObject robotBody)
        {
            _robotBody = robotBody;
            _matrixWidth = (int) (360 / horizontalDensity);
            _matrixHeight = (int) ((verticalFOVBounds[1] - verticalFOVBounds[0]) / verticalDensity) + 1;
            _totalRaycasts = _matrixWidth * _matrixHeight;
            _localRaycastDirections = new NativeArray<Vector3>(
                _totalRaycasts, Allocator.Persistent);
            _distancesPlaceholder = new float[_totalRaycasts];
            _hitsPlaceholder = new RaycastHit[_totalRaycasts];
            
            distanceMatrix = new FloatArrayContainer[_matrixHeight];
            InitDistanceMatrix();

            var robotMain = _robotBody.transform.root.GetComponent<RobotMain>();
            _mask = (LayerMask) robotMain.LidarMask;
            
            Debug.Log(_mask);

            for (var i = 0; i < _matrixHeight; i++)
            for (var j = 0; j < _matrixWidth; j++)
            {
                _localRaycastDirections[i * _matrixWidth + j] =
                    Quaternion.AngleAxis(j * horizontalDensity, Vector3.up)
                    * Quaternion.AngleAxis(verticalFOVBounds[0] + i * verticalDensity, Vector3.right)
                    * Vector3.forward;
            }

            StartRaycastJobs();
            Update();
        }

        public void Update()
        {
            if (!_raycastsHandle.IsCompleted)
            {
                return;
            }
            
            _raycastsHandle.Complete();
            CopyRaycastsToDistanceMatrix();
            CleanupNativeArrays();
            StartRaycastJobs();
        }

        private void CleanupNativeArrays()
        {
            _raycastHits.Dispose();
            _commands.Dispose();
        }

        private void CopyRaycastsToDistanceMatrix()
        {
            var distances = GetRaycastDistancesFromHits();
            
            for (var i = 0; i < _matrixHeight; i++)
            {
                Array.Copy(distances,
                    i * _matrixWidth, distanceMatrix[i].array,
                    0, _matrixWidth);
            }
        }

        private float[] GetRaycastDistancesFromHits()
        {
            _raycastHits.CopyTo(_hitsPlaceholder);
            for (var i = 0; i < _totalRaycasts; i++)
            {
                var hit = _hitsPlaceholder[i];
                if (ReferenceEquals(hit.collider, null))
                    _distancesPlaceholder[i] = range;
                else
                    _distancesPlaceholder[i] = hit.distance;
            }
            
            return _distancesPlaceholder;
        }

        private void InitDistanceMatrix()
        {
            for (var i = 0; i < _matrixHeight; i++)
                distanceMatrix[i] = new FloatArrayContainer(new float[_matrixWidth]);
        }

        private NativeArray<Vector3> GetWorldRaycastDirections()
        {
            var worldDirections = new NativeArray<Vector3>(_totalRaycasts, Allocator.TempJob);
            
            var rotaterJob = new MatrixRotater();
            rotaterJob.objectWorldRotation = _robotBody.transform.rotation;
            rotaterJob.localDirections = _localRaycastDirections;
            rotaterJob.worldDirections = worldDirections;

            var rotaterHandle = rotaterJob.Schedule(_totalRaycasts, 10);
            rotaterHandle.Complete();

            return worldDirections;
        }

        private NativeArray<RaycastCommand> GetRaycastCommands()
        {
            var worldDirections = GetWorldRaycastDirections();
            _commands = new NativeArray<RaycastCommand>(_totalRaycasts, Allocator.TempJob);

            var robotPos = _robotBody.transform.position;
            for (var i = 0; i < _totalRaycasts; i++)
                _commands[i] = new RaycastCommand(robotPos, worldDirections[i], 
                    range, _mask, 1);

            worldDirections.Dispose();
            return _commands;
        }

        private void StartRaycastJobs()
        {
            _raycastHits = new NativeArray<RaycastHit>(_totalRaycasts, Allocator.TempJob);
            
            var commands = GetRaycastCommands();
            _raycastsHandle = RaycastCommand.ScheduleBatch(commands, _raycastHits, 1);
        }
        
        [BurstCompile]
        private struct MatrixRotater : IJobParallelFor
        {
            [ReadOnly] public Quaternion objectWorldRotation;
            [ReadOnly] public NativeArray<Vector3> localDirections;
            [WriteOnly] public NativeArray<Vector3> worldDirections;

            public void Execute(int index)
            {
                worldDirections[index] = objectWorldRotation * localDirections[index];
            }
        }

        /*public void Update()
        {
            for (var i = 0; i < distanceMatrix.Length; i++)
            for (var j = 0; j < distanceMatrix[0].array.Length; j++)
            {
                var rayDirectionLocal =
                    Quaternion.AngleAxis(j * horizontalDensity, Vector3.up)
                    * Quaternion.AngleAxis(verticalFOVBounds[0] + i * verticalDensity, Vector3.right)
                    * Vector3.forward;

                var rayDirectionWorld = _robotBody.transform.TransformDirection(rayDirectionLocal);

                if (Physics.Raycast(_robotBody.transform.position, rayDirectionWorld, out var hit, range, _lidarMask))
                    distanceMatrix[i].array[j] = hit.distance;
                else
                    distanceMatrix[i].array[j] = range;
            }
        }

        private void InitDistanceMatrix()
        {
            distanceMatrix = new FloatArrayContainer
                [(int) ((-verticalFOVBounds[0] + verticalFOVBounds[1]) / verticalDensity + 0.5f) + 1];

            var horizontalSamples = (int) (360.0 / horizontalDensity + 0.5f);
            for (var i = 0; i < distanceMatrix.Length; i++)
                distanceMatrix[i] = new FloatArrayContainer(new float[horizontalSamples]);
        }*/
    }
}