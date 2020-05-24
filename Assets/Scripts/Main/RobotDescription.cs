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

        private ClassicTagDirector _classicTagScript;
        private int _id;

        public Gyroscope gyroscope;
        public Lidar lidar;
        public GPS gps;
        public Altimeter altimeter;
        public Radar radar;
        public float timestamp;
        public bool isIt;
        public string gameMode;

        public RobotDescription(GameObject gameObject, GameModeEnum gameMode, int actorNumber = 0, ClassicTagDirector classicTagScript = null)
        {

            lidar = new Lidar(gameObject);
            gyroscope = new Gyroscope(gameObject);
            gps = new GPS(gameObject);
            altimeter = new Altimeter(gameObject);
            radar = new Radar(gameObject, gameMode);
            this.gameMode = new GameModeEnumWrapper
            {
                Index = (int) gameMode
            }.ToString();

            _classicTagScript = classicTagScript;
            _id = actorNumber;


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
            {
                isIt = _classicTagScript.currentItActorNumber == _id;
            }

        }
        
    }
}