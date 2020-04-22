using System;
using UnityEngine;

namespace Main.Sensor
{
    [Serializable]
    public class Altimeter : ISensor
    {
        
        public float altitude;
        private readonly GameObject _robot;

        public Altimeter(GameObject robot)
        {

            _robot = robot;

        }

        public void Update()
        {

            var pos = _robot.transform.position;
            altitude = pos.y - Terrain.activeTerrain.SampleHeight(pos);

        }

    }
}