using System;
using System.Collections.Generic;
using GameDirection;
using Networking;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class Radar : ISensor
    {
        private ClassicTagDirector _classicTagScript;
        private GameModeEnum _gameMode;
        private PlayerConnection _playerConnection;

        private GameObject _robotBody;
        public Vector3 itPing;

        public Vector3[] pings;

        public Radar(GameObject robot, GameModeEnum gameMode)
        {
            _robotBody = robot;
            _gameMode = gameMode;

            if (_gameMode == GameModeEnum.ClassicTag)
            {
                var robotRoot = _robotBody.transform.root.gameObject;
                _classicTagScript = GameObject.FindGameObjectWithTag("GameDirector").GetComponent<ClassicTagDirector>();
                _playerConnection = robotRoot.GetComponent<RobotNetworkBridge>().playerConnection;
            }
        }

        public void Update()
        {
            var tmpPingList = new List<Vector3>();
            foreach (var robot in GameObject.FindGameObjectsWithTag("Robot"))
                if (robot.GetInstanceID() != _robotBody.transform.parent.gameObject.GetInstanceID())
                {
                    var delta = robot.transform.position - _robotBody.transform.position;
                    tmpPingList.Add(delta);
                }

            pings = tmpPingList.ToArray();

            if (_gameMode == GameModeEnum.ClassicTag && _classicTagScript.currentItActorNumber != -1)
                // we've assigned an it
            {
                var it = _playerConnection.robots[_classicTagScript.currentItActorNumber];

                if (!it)
                    _playerConnection.robots.Remove(_classicTagScript.currentItActorNumber);

                itPing = _playerConnection.robots[_classicTagScript.currentItActorNumber]
                    .transform.position - _robotBody.transform.position;
            }
        }
    }
}