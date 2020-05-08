using System;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class GPS : ISensor
    {

        private GameObject _robot;
        private Rigidbody _robotRigidbody;

        public Vector3 position;
        public Vector3 velocity;

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