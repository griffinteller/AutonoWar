using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using GameDirection;
using Networking;
using Photon.Pun;
using UnityEngine;
using Utility;

namespace Main
{
    public class RobotStateSender : MonoBehaviour
    {
        private const string MessageSeparator = ";";

        private static readonly Action<object> ConnectUpdateAndWritePosix = rss =>
        {
            var rssCast = (RobotStateSender) rss;

            try
            {
                var robotDescriptionBytes = GetRobotDescriptionBytes(rssCast._robotStateDescription); // json

                while (true)
                    try
                    {
                        rssCast._clientStream =
                            new FileStream("/tmp/" + rssCast.PipeName, FileMode.Open, FileAccess.Write);
                        break;
                    }
                    catch (IOException e)
                    {
                        Debug.Log("Couldn't connect rss: " + e);
                    }

                rssCast._clientStream.Write(robotDescriptionBytes, 0, robotDescriptionBytes.Length);
                rssCast._clientStream.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                rssCast._clientStream.Close();
            }
        };

        private Stream _clientStream;
        private bool _connected;
        private Task _currentWriteTask;
        private SimplePlatform _platform;
        private RobotMain _robotMain;

        private RobotDescription _robotStateDescription;

        private string PipeName = "RobotInfoPipe";
        public GameObject robotBody;

        public void Start()
        {
            _platform = SystemUtility.GetSimplePlatform();

            GetRoomVariables(out var gameMode, out var map, out var actorNumber);
            _robotStateDescription = new RobotDescription(robotBody, gameMode, map, actorNumber);

            _robotMain = GetComponent<RobotMain>();
            PipeName += _robotMain.robotIndex;

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

                    _currentWriteTask = Task.Factory.StartNew(ConnectUpdateAndWritePosix, this);
                    break;

                default:

                    throw new NotImplementedException();
            }
        }

        public void FixedUpdate()
        {
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
                return; // therefore the API is not running

            _robotStateDescription.Update();

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
            _robotStateDescription.Update();

            if (!(_currentWriteTask.Status == TaskStatus.Canceled
                  || _currentWriteTask.Status == TaskStatus.Faulted
                  || _currentWriteTask.IsCompleted))
                return;

            _currentWriteTask = Task.Factory.StartNew(ConnectUpdateAndWritePosix, this);
        }

        private static byte[] GetRobotDescriptionBytes(RobotDescription robotDescriptionDescription)
        {
            return Encoding.ASCII.GetBytes(
                JsonUtility.ToJson(robotDescriptionDescription) + MessageSeparator);
        }

        private static string GetRobotDescription(RobotDescription robotDescriptionDescription)
        {
            return JsonUtility.ToJson(robotDescriptionDescription);
        }

        private void GetRoomVariables(
            out GameModeEnum gameMode,
            out MapEnum map,
            out int actorNumber)
        {
            var gameDirector = FindObjectOfType<GameDirector>();

            gameMode = gameDirector.GameMode;
            map = gameDirector.CurrentMap;
            actorNumber = -1;

            if (!PhotonNetwork.InRoom) 
                return;
            
            var robotNetworkBridge = GetComponent<RobotNetworkBridge>();
            actorNumber = robotNetworkBridge.actorNumber;
        }
    }
}