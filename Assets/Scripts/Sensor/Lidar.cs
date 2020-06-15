using System;
using Main;
using UnityEngine;
using Utility;

namespace Sensor
{
    [Serializable]
    public class Lidar : ISensor
    {
        private const float UpdateInterval = 0.2f;
        private ActionHandler _actionHandler;
        private float _lastUpdate;
        private LayerMask _lidarMask;

        private GameObject _robotBody;

        public FloatArrayContainer[] distanceMatrix;

        public float horizontalDensity;

        public float range;
        public float verticalDensity;
        public float[] verticalFOVBounds; // how many degrees down, how many up

        public Lidar(GameObject robotBody)
        {
            _lidarMask = LayerMask.GetMask("LidarVisible", "Robots");
            _robotBody = robotBody;
            _actionHandler = _robotBody.transform.root.GetComponent<ActionHandler>();
            horizontalDensity = 5;
            verticalDensity = 5;
            verticalFOVBounds = new float[] {-30, 30};
            _lastUpdate = -UpdateInterval;

            range = 300;

            InitDistanceMatrix();

            Update();
        }

        public void Update()
        {
            if (Time.time - _lastUpdate < UpdateInterval)
                return;

            _lastUpdate = Time.time;

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
        }
    }
}