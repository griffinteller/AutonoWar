using System;
using System.Collections.Generic;
using Main;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Sensor
{
    [Serializable]
    public class Radar : ISensor
    {

        private GameObject _robotBody;
        private string _gamemode;
        private ClassicTagScript _classicTagScript;
        private PlayerConnection _playerConnection;

        public Vector3[] pings;
        public Vector3 itPing;

        public Radar(GameObject robot, string gamemode)
        {

            _robotBody = robot;
            _gamemode = gamemode;

            if (_gamemode.Equals("Classic Tag"))
            {

                var robotRoot = _robotBody.transform.root.gameObject;
                _classicTagScript = robotRoot.gameObject.GetComponent<ClassicTagScript>();
                _playerConnection = robotRoot.GetComponent<RobotNetworkBridge>().playerConnection;

            }

        }

        public void Update()
        {

            var tmpPingList = new List<Vector3>();
            if (PhotonNetwork.IsConnected)
            {

                foreach (var robot in GameObject.FindGameObjectsWithTag("Robot"))
                {

                    if (robot.GetInstanceID() != _robotBody.transform.parent.gameObject.GetInstanceID())
                    {

                        var delta = robot.transform.position - _robotBody.transform.position;
                        tmpPingList.Add(delta);

                    }

                }
                
            }
            
            pings = tmpPingList.ToArray();

            if (_gamemode.Equals("Classic Tag") && _classicTagScript.currentItActorNumber != -1) // we've assigned an it
            {

                itPing = _playerConnection.robots[_classicTagScript.currentItActorNumber]
                    .transform.position - _robotBody.transform.position;

            }

        }
    }
}