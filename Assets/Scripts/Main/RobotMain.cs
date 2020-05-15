using System;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Main
{

    public class RobotMain : MonoBehaviour
    {
        
        public int actorNumber;
        
        private const float MaxAngularVelocity = 500f;

        public void Start()
        {

            SetMaximumAngularVelocities();

            InitializeScripts();

        }

        private void InitializeScripts()
        {

            if (PhotonNetwork.InRoom)
            {
                GetComponent<RobotNetworkBridge>().enabled = true;
            }
            else
            {
                InitializeSinglePlayerScripts();
            }
            
        }

        private void InitializeSinglePlayerScripts()
        {
            GetComponent<UserScriptInterpreter>().enabled = true;
            GetComponent<RobotStateSender>().enabled = true;
            GetComponent<DesignLoaderPlay>().enabled = true;
            
            GetComponent<DesignLoaderPlay>().BuildRobotSinglePlayer();
        }

        private void SetMaximumAngularVelocities()
        {

            var rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (var rigidbody in rigidbodies)
            {

                rigidbody.maxAngularVelocity = MaxAngularVelocity;

            }

        }
    }
}