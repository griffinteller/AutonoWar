using System;
using UnityEngine;
using Utility;

namespace Sensor
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
            altitude = pos.y - TerrainUtility.GetClosestCurrentTerrain(pos).SampleHeight(pos);

        }

    }
}