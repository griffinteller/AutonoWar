using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Networking;
using Photon.Pun;
using UnityEngine;
using Utility;

namespace Main
{
    public class RobotStateSender : MonoBehaviour
    {

        public GameObject robotBody;

        private Robot _robotStateDescription;
        private Stream _clientStream;
        private bool _connected;
        private SimplePlatform _platform;
        
        private const string PipeName = "RobotInfoPipe";
        private const string MessageSeparator = ";";
        
        private static readonly Action<object> InitConnectToPipePosix = rss =>
        {

            var rssCast = (RobotStateSender) rss;

            rssCast._clientStream = File.OpenWrite("/tmp/" + PipeName);
            rssCast._connected = true;
            
        };

        public void Start()
        {

            _platform = SystemUtility.GetSimplePlatform();
            
            GetRoomVariables(out var gamemode, out var actorNumber, out var classicTagScript);
            _robotStateDescription = new Robot(robotBody, gamemode, actorNumber, classicTagScript);

            InitStream();
            
        }

        private void InitStream()
        {
            
            switch (_platform)
            {
                
                case SimplePlatform.Windows:
                    
                    _clientStream = new NamedPipeClientStream(PipeName);
                    break;
                
                case SimplePlatform.Posix:

                    Task.Factory.StartNew(InitConnectToPipePosix, this);
                    break;
                
                default:
                    
                    throw new NotImplementedException();
                
            }
            
        }

        public void Update()
        {
            
            _robotStateDescription.Update();

            switch (_platform)
            {
                
                case SimplePlatform.Windows:
                    
                    WindowsUpdate();
                    break;
                
                case SimplePlatform.Posix:

                    PosixUpdate();
                    break;
                
                default:
                    
                    throw new NotImplementedException();
                
            }

        }

        private void WindowsUpdate()
        {
            
            if (!_connected && 
                !SystemUtility.TryConnectPipeClientWindows((NamedPipeClientStream) _clientStream, out _connected))  
                // we are not connected and we can't connect
                return;  // therefore the API is not running

            try
            {

                var robotDescriptionBytes = GetRobotDescriptionBytes(_robotStateDescription);
                _clientStream.Write(robotDescriptionBytes, 0, robotDescriptionBytes.Length);

            }
            catch (IOException) // server has stopped
            {
                
                _connected = false;
                _clientStream = new NamedPipeClientStream(PipeName); // restart pipe to connect again later
                
            }
            
        }

        private void PosixUpdate()
        {

            if (!_connected)
                return;

            var robotDescriptionBytes = GetRobotDescriptionBytes(_robotStateDescription); // json
            _clientStream.Write(robotDescriptionBytes, 0, robotDescriptionBytes.Length);
            _clientStream.Close();
            _connected = false;
            Task.Factory.StartNew(InitConnectToPipePosix, this);


        }

        private static byte[] GetRobotDescriptionBytes(Robot robotDescription)
        {
            
            return Encoding.ASCII.GetBytes(
                JsonUtility.ToJson(robotDescription) + MessageSeparator);
            
        }

        private void GetRoomVariables(
            out string gamemode,
            out int actorNumber,
            out ClassicTagScript classicTagScript)
        {

            gamemode = "Singleplayer";
            actorNumber = -1;
            classicTagScript = null;
            
            if (PhotonNetwork.InRoom)
            {

                gamemode = (string) PhotonNetwork.CurrentRoom.CustomProperties["Gamemode"];
                if (gamemode.Equals("Classic Tag"))
                {

                    classicTagScript = GetComponent<ClassicTagScript>();

                    var robotNetworkBridge = GetComponent<RobotNetworkBridge>();
                    actorNumber = robotNetworkBridge.actorNumber;

                }

            }

        }
        
    }
}
