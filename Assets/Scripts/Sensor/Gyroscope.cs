using System;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class Gyroscope
    {

        private GameObject _robotBody;

        public Vector3 forward;
        public Vector3 right;
        public Vector3 up;

        public bool isUpsideDown;

        public Gyroscope(GameObject robotBody)
        {

            _robotBody = robotBody;
            Update();

        }

        public void Update()
        {

            var robotBodyTransform = _robotBody.transform;
                
            forward = robotBodyTransform.forward;
            right = robotBodyTransform.right;
            up = robotBodyTransform.up;

            isUpsideDown = up.y < 0;

        }

    }
}