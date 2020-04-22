using System;
using JetBrains.Annotations;
using Main.Sensor;
using UnityEngine;
using Utility;

using Gyroscope = Main.Sensor.Gyroscope;

namespace Main
{
    [Serializable]
    public class Robot
    {

        private ClassicTagScript _classicTagScript;
        private int _id;

        public Gyroscope gyroscope;
        public Lidar lidar;
        public GPS gps;
        public Altimeter altimeter;
        public Radar radar;
        public long timestamp;
        public bool isIt;
        public string gameMode;
        
        public Robot(GameObject gameObject, string gamemode, int actorNumber = 0, ClassicTagScript classicTagScript = null)
        {
            
            lidar = new Lidar(gameObject);
            gyroscope = new Gyroscope(gameObject);
            gps = new GPS(gameObject);
            altimeter = new Altimeter(gameObject);
            radar = new Radar(gameObject, gamemode);
            gameMode = gamemode;

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
            timestamp = TimeUtility.CurrentTimeMillis();

            if (gameMode.Equals("Classic Tag"))
            {

                isIt = _classicTagScript.currentItActorNumber == _id;

            }

        }
        
    }
}