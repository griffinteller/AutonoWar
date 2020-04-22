using System;
using UnityEngine;

namespace Main.Sensor
{
    [Serializable]
    public class Gyroscope
    {

        private GameObject _robot;

        public int orientationNegation = 1;

        public Vector3 forward;
        public Vector3 right;
        public Vector3 up;

        public Gyroscope(GameObject robot)
        {

            _robot = robot;
            Update();

        }

        public void Update()
        {
                
            forward = _robot.transform.forward;
            right = _robot.transform.right * orientationNegation;
            up = _robot.transform.up * orientationNegation;

        }

    }
}