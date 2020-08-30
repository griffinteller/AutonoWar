using System;
using System.Collections.Generic;
using System.Text;
using Building;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace UI.RobotDisplay
{
    public class RobotDisplay : MonoBehaviour
    {
        public DesignLoaderDisplay displayRobotPrefab;
        public GameObject newRobotObjectPrefab;
        public RobotInfoPanel infoPanel;
        
        private const float Spacing = 8;
        private const float MouseSensitivity = 0.015f;
        private const float SmoothTime = 0.2f;

        private List<RobotStructure> _structures;
        private Dictionary<string, int> _structureIndicesByName = new Dictionary<string, int>();

        private int _lastSelectedIndex;
        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                SetPositionByIndex(value);
            }
        }

        public static Vector3 GetRobotPositionAtIndex(int index)
        {
            return Vector3.right * Spacing * index;
        }

        public void Awake()
        {
            _structures = SystemUtility.GetAllRobotStructures();
            GetStructureIndicesByName();
        }

        private void GetStructureIndicesByName()
        {
            for (var i = 0; i < _structures.Count; i++)
                _structureIndicesByName.Add(_structures[i].name, i);
        }

        public void Start()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefKeys.SelectedRobotNameKey))
                PlayerPrefs.SetString(PlayerPrefKeys.SelectedRobotNameKey, null);
            else
            {
                var robotName = PlayerPrefs.GetString(PlayerPrefKeys.SelectedRobotNameKey);
                if (robotName == null)
                    SelectedIndex = -1;
                else
                    SelectedIndex = _structureIndicesByName[robotName];
            }
            
            InstantiateRobots();
        }

        private void SetPositionByIndex(int index)
        {
            var pos = transform.position;
            pos.x = -(Spacing * index);
            transform.position = pos;
        }

        public void Update()
        {
            KeyCheck();
            
            if (!Input.GetMouseButton(0))
                SnapPosition();

            GenerateInfo();
        }
        
        private void GenerateInfo()
        {
            if (_selectedIndex == _lastSelectedIndex)
                return;
            
            List<(string, string)> info;
            if (_selectedIndex == -1)
            {
                infoPanel.titleText.text = "New Robot";
                info = new List<(string, string)>();
            }
            else
            {
                var currentStructure = _structures[_selectedIndex];

                infoPanel.titleText.text = currentStructure.name;

                info = new List<(string, string)>();
                info.Add(("Mass:", currentStructure.mass.ToString("N1") + " kg"));
                
                var tireStringBuilder = new StringBuilder();
                foreach (var tire in currentStructure.tires)
                {
                    tireStringBuilder.Append(tire);
                    tireStringBuilder.AppendLine();
                }

                var tireString = tireStringBuilder.ToString();
                
                if (!string.IsNullOrEmpty(tireString) 
                    && tireString.Substring(tireString.Length - 1).Equals("\n"))
                    tireString = tireString.Substring(0, tireString.Length - 1);
                
                info.Add(("Tires:", tireString));
            }

            infoPanel.infoList.SetData(info);
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
            _lastSelectedIndex = _selectedIndex;
            
            var t = transform;
            var localPos = t.localPosition;
            
            var currentPosX = localPos.x;
            var desiredPosX = Mathf.Clamp(Mathf.Round(currentPosX / Spacing) * Spacing, 
                -(_structures.Count - 1) * Spacing, Spacing);
            _selectedIndex = Mathf.RoundToInt(-desiredPosX / Spacing);
            
            if (_selectedIndex == -1)
                PlayerPrefs.SetString(PlayerPrefKeys.SelectedRobotNameKey, null);
            else
                PlayerPrefs.SetString(PlayerPrefKeys.SelectedRobotNameKey, _structures[_selectedIndex].name);
            
            var desiredPos = new Vector3(desiredPosX, localPos.y, localPos.z);
            t.localPosition = Vector3.SmoothDamp(localPos, desiredPos, ref _velocity, SmoothTime);
        }

        private void InstantiateRobots()
        {
            var newRobotObject = Instantiate(newRobotObjectPrefab, transform);
            newRobotObject.transform.localPosition = GetRobotPositionAtIndex(-1);
            
            for (var i = 0; i < _structures.Count; i++)
            {
                var structure = _structures[i];
                var robotObj = Instantiate(displayRobotPrefab.gameObject, transform);
                var displayComponent = robotObj.GetComponent<DesignLoaderDisplay>();

                displayComponent.structure = structure;
                robotObj.transform.localPosition = GetRobotPositionAtIndex(i);
            }
        }
        
        public void StartBuildSceneWithSettings(string robotName = null)
        {
            var buildDataObj = new GameObject(
                "BuildData",
                typeof(CrossSceneDataContainer));
            var buildData = buildDataObj.GetComponent<CrossSceneDataContainer>();
            buildData.data.Add("robotName", robotName);

            DontDestroyOnLoad(buildDataObj);

            SceneManager.LoadScene(MetaUtility.BuildSceneName);
        }

        public void DeleteSelectedRobot()
        {
            _structures.RemoveAt(_selectedIndex);

            var t = transform;
            Destroy(t.GetChild(_selectedIndex + 1).gameObject);

            for (var i = _selectedIndex + 1; i < _structures.Count + 1; i++)
            {
                t.GetChild(i + 1).Translate(-Vector3.right * Spacing, Space.World);
            }

            SelectedIndex -= 1;
        }
    }
}