using System;
using System.Collections.Generic;
using System.IO;
using Networking;
using Photon.Pun;
using UnityEngine;
using Utility;

namespace Main
{
    public class ActionHandler : MonoBehaviour
    {

        public GameObject robotBody;

        public int internalNegation = 1; // Wheels should keep spinning in same direction regardless of user coord flipping
        public int actorNumber;

        public string eventQueuePath = "";  // the file where commands from the user are stored
        public string robotStatePath = "";  // the json file where the game prints the robot state

        public string dataDirectory;  // AppData/Local/ABR on Windows, ~/.local/share/ABR on POSIX

        private Vector3 _startingPosition;  // we store these in case we want to reset the robot and we didn't start from 0,0,0
        private Quaternion _startingRotation;

        private WheelCollider[] _wheelColliders;

        private Dictionary<string, GameObject> _gameObjectsCache;  // caching in a hashtable is faster than sequentially searching every time

        private RobotStateSender _robotStateSender;

        private RobotNetworkBridge _robotNetworkBridge;

        // Start is called before the first frame update
        void Start()
        {
            
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;

            _wheelColliders = GetComponentsInChildren<WheelCollider>();

            _robotStateSender = GetComponent<RobotStateSender>();
            
            _gameObjectsCache = new Dictionary<string, GameObject>();
            
            InitDataDirectory(); // get or create data directory
            eventQueuePath = dataDirectory + "EventQueue";
            robotStatePath = dataDirectory + "RobotState.json";

            _robotNetworkBridge = GetComponent<RobotNetworkBridge>();
            
            if (PhotonNetwork.InRoom)
            {

                _robotNetworkBridge.enabled = true;

            }
            else
            {
                
                GetComponent<UserScriptInterpreter>().enabled = true;
                GetComponent<RobotStateSender>().enabled = true;

            }

        }

        // flips user coordinates around forward axis to facilitate steering and prevent turtleing, if the user wants
        /*public void FlipUserCoordinates()
        {
            
            userNegation *= -1;
            _robotStateSender.robotStateDescription.gyroscope.orientationNegation *= -1; // flip user coordinates

        }*/

        // gets and/or creates directory for game files
        private void InitDataDirectory()
        {

            dataDirectory = SystemUtility.GetAndCreateDataDirectory();

        }

        public void ResetRobot()
        {
            
            transform.position = _startingPosition;
            transform.rotation = _startingRotation;
            
            transform.GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(0).localRotation = Quaternion.identity;
            
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            ResetInternalState();

        }

        // set torques to zero and steering to 0
        public void ResetInternalState()
        {

            foreach (var wheelCollider in _wheelColliders)
            {

                wheelCollider.motorTorque = 0;
                wheelCollider.steerAngle = 0;

            }

            //userNegation = 1;
            robotBody.transform.localRotation = Quaternion.identity;

        }
        
        public GameObject CachedFind(string name)
        {
            
            GameObject obj;
            try
            {

                obj = _gameObjectsCache[name];

            }
            catch (KeyNotFoundException)
            {

                obj = GameObject.Find(name);
                _gameObjectsCache.Add(name, obj);
                
            }

            return obj;

        }

        public void SetTireTorque(string tireName, float torque)
        {

            var tireObject = CachedFind(tireName);
            tireObject.GetComponent<WheelCollider>().motorTorque = torque * internalNegation; //* userNegation;

        }

        public void InvertTires()
        {

            foreach (var wheelCollider in _wheelColliders)
            {

                wheelCollider.motorTorque *= -1;
                wheelCollider.steerAngle *= -1;

            }
            
        }

        public void SetTireSteering(string tireName, float bering)
        {
            
            var tireObject = CachedFind(tireName);
            tireObject.GetComponent<WheelCollider>().steerAngle = internalNegation * bering;
            CachedFind(tireName + "Vis").transform.localRotation = Quaternion.Euler(
                0, bering, 0);

        }

        public void OnDestroy()
        {
            
            if (PhotonNetwork.InRoom)
            {

                var playerConnection = GameObject.FindWithTag("ConnectionObject").GetComponent<PlayerConnection>();
                playerConnection.robots.Remove(actorNumber);

            }
            
        }
    }

}