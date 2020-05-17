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
        private const float TagBuffer = 0.3f;

        public void Start()
        {

            SetMaximumAngularVelocities();

            InitializeScripts();

        }

        public void AddSphereTrigger()
        {

            var bounds = new Bounds();
            var colliders = GetComponentsInChildren<Collider>();
            
            foreach (var collider in colliders)
                bounds.Encapsulate(collider.bounds.extents + collider.transform.localPosition);

            var sphereCollider = gameObject.AddComponent<SphereCollider>();

            sphereCollider.radius = bounds.extents.magnitude + TagBuffer;
            sphereCollider.isTrigger = true;

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