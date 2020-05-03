using System;
using UnityEngine;
using Utility;

namespace Main.Sensor
{
    [Serializable]
    public class GPS : ISensor
    {

        private GameObject _robot;
        private Rigidbody _robotRigidbody;

        public Vector3 position;
        public Vector3 velocity;

        private long _lastTime = 0;
        private Vector3 _lastPos;

        public GPS(GameObject robot)
        {

            _robot = robot;

            var robotRoot = robot.transform.root;
            _robotRigidbody = robotRoot.GetComponent<Rigidbody>();
            
            Update();

        }

        public void Update()
        {

            position = _robot.transform.position;
            velocity = _robotRigidbody.velocity;

        }

    }
}