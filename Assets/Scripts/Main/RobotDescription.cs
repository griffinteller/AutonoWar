using System;
using GameDirection;
using Networking;
using Sensor;
using UnityEngine;
using Gyroscope = Sensor.Gyroscope;

namespace Main
{
    [Serializable]
    public class RobotDescription
    {
        private int _id;
        private GameDirector _gameDirector;
        private Type _gameDirectorType;
        private DefaultCycleGameDirector _cycledGameDirector;
        
        public Altimeter altimeter;
        public string gameMode;
        public string map;
        public GPS gps;

        public Gyroscope gyroscope;
        public bool isIt;
        public Lidar lidar;
        public Radar radar;
        public float timestamp;
        public bool hasGameStarted;

        public RobotDescription(GameObject gameObject, GameModeEnum gameMode, MapEnum map, int actorNumber = 0)
        {
            lidar = new Lidar(gameObject);
            gyroscope = new Gyroscope(gameObject);
            gps = new GPS(gameObject);
            altimeter = new Altimeter(gameObject);
            radar = new Radar(gameObject);
            this.gameMode = new GameModeEnumWrapper
            {
                Index = (int) gameMode
            }.ToString();
            this.map = new MapEnumWrapper
            {
                Index = (int) map
            }.ToString();
            
            _id = actorNumber;
            _gameDirector = UnityEngine.Object.FindObjectOfType<GameDirector>();
            _gameDirectorType = _gameDirector.GetType();
            
            _cycledGameDirector = _gameDirector as DefaultCycleGameDirector;  // null if not a cycled game director
        }

        public void Update()
        {
            gyroscope.Update();
            lidar.Update();
            gps.Update();
            altimeter.Update();
            radar.Update();
            timestamp = Time.fixedTime * 1000f;

            if (gameMode.Equals("Classic Tag")) 
                isIt = ((ClassicTagDirector) _gameDirector).currentItActorNumber == _id;

            CheckIfGameStarted();
        }

        private void CheckIfGameStarted()
        {
            if (_gameDirectorType.IsSubclassOf(typeof(DefaultCycleGameDirector)))
                hasGameStarted = _cycledGameDirector.gameState == GameState.Started;
            else
                hasGameStarted = true;
        }
    }
}