using System;
using System.Collections.Generic;
using GameDirection;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class Radar : ISensor
    {
        private GameDirector _gameDirector;
        private PlayerConnection _playerConnection;

        private GameObject _robotBody;
        public Vector3 itPing;
        public Vector3[] pings;

        public Radar(GameObject robot)
        {
            _robotBody = robot;

            var robotRoot = _robotBody.transform.root.gameObject;
            _gameDirector = UnityEngine.Object.FindObjectOfType<GameDirector>();

            SetupGameModeSpecificSettings();
            
            if (PhotonNetwork.InRoom)
                _playerConnection = robotRoot.GetComponent<RobotNetworkBridge>().playerConnection;
        }

        public void Update()
        {
            UpdatePingArray();

            switch (_gameDirector)
            {
                case ClassicTagDirector classicTagDirector:
                    ClassicTagUpdate(classicTagDirector);
                    break;
            }
        }

        private void SetupGameModeSpecificSettings()
        {
            switch (_gameDirector)
            {
            }
        }

        private void ClassicTagUpdate(ClassicTagDirector classicTagDirector)
        {
            if (classicTagDirector.currentItActorNumber == -1)
                return;

            itPing = _playerConnection.robots[classicTagDirector.currentItActorNumber]
                .transform.position - _robotBody.transform.position;
        }

        private void UpdatePingArray()
        {
            var tmpPingList = new List<Vector3>();
            foreach (var robot in GameObject.FindGameObjectsWithTag("Robot"))
                if (robot.GetInstanceID() != _robotBody.transform.parent.gameObject.GetInstanceID())
                {
                    var delta = robot.transform.position - _robotBody.transform.position;
                    tmpPingList.Add(delta);
                }

            pings = tmpPingList.ToArray();
        }
    }
}