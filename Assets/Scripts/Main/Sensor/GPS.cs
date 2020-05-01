using System;
using UnityEngine;

namespace Main.Sensor
{
    [Serializable]
    public class GPS : ISensor
    {

        private GameObject _robot;    

        public Vector3 position;

        public GPS(GameObject robot)
        {

            _robot = robot;
            Update();

        }

        public void Update()
        {

            position = _robot.transform.position;

        }

    }
}