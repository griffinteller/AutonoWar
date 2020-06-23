using System;
using GameDirection;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class GPS : ISensor
    {
        private GameObject _robot;
        private Rigidbody _robotRigidbody;
        private GameDirector _gameDirector;

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 destination;

        public GPS(GameObject robot)
        {
            _robot = robot;

            var robotRoot = robot.transform.root;
            _robotRigidbody = robotRoot.GetComponent<Rigidbody>();
            _gameDirector = UnityEngine.Object.FindObjectOfType<GameDirector>();

            var grandPrixDirector = _gameDirector as GrandPrixDirector;
            if (grandPrixDirector)
                destination = grandPrixDirector.Endpoint;

            Update();
        }

        public void Update()
        {
            position = _robot.transform.position;
            velocity = _robotRigidbody.velocity;
        }
    }
}