using System.Collections.Generic;
using Cam;
using Main;
using Networking;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utility;
using Random = UnityEngine.Random;

namespace GameDirection
{

    public class SinglePlayerDirector : GameDirector
    {
        public override GameModeEnum GameMode => GameModeEnum.SinglePlayer;

        public int numControllableBots;
        public MapEnum map;
        public GameObject robotParentPrefab;
        public Vector3 startingPosition = new Vector3(-268, 0, -410);
        public float maxPlacementRadius = 20f;
        public float startingHeight = 3f;

        private InGameUIScript _inGameUiScript;
        private CameraMotionScript _cameraMotionScript;
        
        public int SelectedRobot
        {
            get => _selectedRobot;
            set
            {
                _selectedRobot = value % numControllableBots;
                InternalSelectRobot(_selectedRobot);
            }
        }

        private int _selectedRobot;

        private readonly List<RobotMain> _robotMains = new List<RobotMain>();

        public void Init()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _1, LoadSceneMode _2)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            _inGameUiScript = FindObjectOfType<InGameUIScript>();
            _cameraMotionScript = Camera.main.GetComponent<CameraMotionScript>();

            for (var i = 0; i < numControllableBots; i++)
                PlaceRobot(i);

            SelectedRobot = 0;
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Destroy(gameObject);
        }

        private void PlaceRobot(int index)
        {
            var xDisplacement = (Random.value * 2 - 1) 
                * maxPlacementRadius / Mathf.Sqrt(2); // +/- rad
            var zDisplacement = (Random.value * 2 - 1)
                * maxPlacementRadius / Mathf.Sqrt(2);
            
            var pos = new Vector3(
                startingPosition.x + xDisplacement, 
                0, 
                startingPosition.z + zDisplacement);

            pos.y = TerrainUtility.GetClosestCurrentTerrain(pos).SampleHeight(pos) 
                    + startingHeight;

            var robot = Instantiate(
                robotParentPrefab, pos,
                Quaternion.Euler(0, Random.value * 360, 0));
            
            _robotMains.Add(robot.GetComponent<RobotMain>());
            _robotMains[_robotMains.Count - 1].robotIndex = index;
        }

        public void SelectNextRobot()
        {
            SelectedRobot += 1;
        }

        private void InternalSelectRobot(int index)
        {
            _cameraMotionScript.SetCenterObject(_robotMains[index].gameObject);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                SelectNextRobot();
        }

        public override List<EscapeMenuButtonInfo> GetEscapeMenuButtonInfo()
        {
            var result = base.GetEscapeMenuButtonInfo();
            result.Add(
                new EscapeMenuButtonInfo("Reset Robot", MetaUtility.UnityEventFromFunc(ResetLocalRobot)));
            
            return result;
        }

        public RobotMain GetSelectedRobot()
        {
            return _robotMains[SelectedRobot];
        }

        public RobotMain GetRobot(int index)
        {
            return _robotMains[index];
        }
    }
}