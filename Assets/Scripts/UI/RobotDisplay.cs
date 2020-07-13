using System;
using System.Collections.Generic;
using System.IO;
using Building;
using UnityEngine;
using Utility;

namespace UI
{
    public class RobotDisplay : MonoBehaviour
    {
        public DesignLoaderDisplay displayRobotPrefab;
        
        private const float Spacing = 8;

        private RobotStructure[] _structures;

        public static Vector3 GetRobotPositionAtIndex(int index)
        {
            return Vector3.right * Spacing * index;
        }

        public void Awake()
        {
            _structures = SystemUtility.GetAllRobotStructures();
        }

        public void Start()
        {
            InstantiateRobots();
        }

        private void InstantiateRobots()
        {
            for (var i = 0; i < _structures.Length; i++)
            {
                var structure = _structures[i];
                var robotObj = Instantiate(displayRobotPrefab.gameObject, transform);
                var displayComponent = robotObj.GetComponent<DesignLoaderDisplay>();

                displayComponent.structure = structure;
                robotObj.transform.position = GetRobotPositionAtIndex(i);
            }
        }
    }
}