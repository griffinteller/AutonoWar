using System;
using System.Collections.Generic;
using System.IO;
using Building;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class RobotDisplay : MonoBehaviour
    {
        public DesignLoaderDisplay displayRobotPrefab;
        
        private const float Spacing = 8;
        private const float MouseSensitivity = 0.015f;
        private const float SmoothTime = 0.2f;

        private RobotStructure[] _structures;
        private int _selectedIndex;
        
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

        public void Update()
        {
            KeyCheck();
            
            if (!Input.GetMouseButton(0))
                SnapPosition();
        }

        private Vector3 _lastMousePos;
        private void KeyCheck()
        {
            if (Input.GetMouseButtonDown(0))
                _lastMousePos = Input.mousePosition;
            
            if (Input.GetMouseButton(0))
            {
                transform.Translate((Input.mousePosition - _lastMousePos).x * MouseSensitivity, 0, 0);
                _lastMousePos = Input.mousePosition;
            }
        }

        private Vector3 _velocity;
        private void SnapPosition()
        {
            var t = transform;
            var localPos = t.localPosition;
            
            var currentPosX = localPos.x;
            var desiredPosX = Mathf.Clamp(Mathf.Round(currentPosX / Spacing) * Spacing, 
                -(_structures.Length - 1) * Spacing, 0);
            _selectedIndex = Mathf.RoundToInt(-desiredPosX / Spacing);
            
            var desiredPos = new Vector3(desiredPosX, localPos.y, localPos.z);
            t.localPosition = Vector3.SmoothDamp(localPos, desiredPos, ref _velocity, SmoothTime);
        }

        private void InstantiateRobots()
        {
            for (var i = 0; i < _structures.Length; i++)
            {
                var structure = _structures[i];
                var robotObj = Instantiate(displayRobotPrefab.gameObject, transform);
                var displayComponent = robotObj.GetComponent<DesignLoaderDisplay>();

                displayComponent.structure = structure;
                robotObj.transform.localPosition = GetRobotPositionAtIndex(i);
            }
        }
    }
}