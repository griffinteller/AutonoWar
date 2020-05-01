using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utility;

namespace Main
{

    public class UserScriptInterpreter : MonoBehaviour
    {

        private readonly object _eventListLock = new object();
        private readonly object _clientStreamLock = new object();
        private readonly List<string> _eventList = new List<string>();
        
        private ActionHandler _actionHandler;
        private SimplePlatform _platform;
        private Task _currentGetEventsTask;
        private CancellationTokenSource _getEventsCancellationTokenSource;
        private Task _currentVerifyConnectionTask;
        private Stream _clientStream;
        private StreamReader _clientReader;

        private bool _connected;
        private bool _justStarted = true;

        private const string PipeName = "EventQueuePipe";
        private const char Separator = ';';
        private const int PosixReadTimeout = 5;
        
        private static readonly Action<object> GetEventsActionWindows = usi =>
        {

            var usiCast = (UserScriptInterpreter) usi;

            var eventList = usiCast._eventList;
            var eventListLock = usiCast._eventListLock;

            lock (usiCast._clientStreamLock)
            {
                
                var reader = usiCast._clientReader;
            
                var message = reader.ReadLine();
                while (!string.IsNullOrEmpty(message))
                {
                    
                    var commands = message.Split(Separator);

                    lock (eventListLock)
                    {
                
                        eventList.AddRange(commands);
                
                    }
                    message = reader.ReadLine();

                }

            }

        };

        private static readonly Action<object> VerifyConnectionActionWindows = usi =>
        {

            var usiCast = (UserScriptInterpreter) usi;
            var stillConnected = SystemUtility.IsDuplexPipeStillConnectedWindows((NamedPipeClientStream) usiCast._clientStream);

            if (stillConnected)
                return;

            Debug.Log("Disconnected!");
            lock (usiCast._clientStreamLock)
            {
                
                usiCast._connected = false;
                usiCast._clientReader.Close();
                usiCast.InitStreams();

            }

        };

        private static readonly Action<object> GetEventsActionPosix = usi =>
        {

            var usiCast = (UserScriptInterpreter) usi;

            var batch = usiCast._clientReader.ReadLine();
            while (!string.IsNullOrEmpty(batch))
            {

                var events = batch.Split(Separator);

                lock (usiCast._eventListLock)
                {
                    usiCast._eventList.AddRange(events);
                }

                batch = usiCast._clientReader.ReadLine();

            }

        };

        private static readonly Action<object> ConnectToPipePosix = usi =>
        {

            var usiCast = (UserScriptInterpreter) usi;

            usiCast._clientStream = File.OpenRead("/tmp/" + PipeName);
            usiCast._clientStream.ReadTimeout = PosixReadTimeout;
            usiCast._connected = true;


        };

        public void Start()
        {

            _platform = SystemUtility.GetSimplePlatform();
            _actionHandler = GetComponent<ActionHandler>();
            
            InitStreams();

        }

        public void Update()
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
            
            ExecuteEvents();

        }

        private void InitStreams()
        {
            
            switch (_platform)
            {
                
                case SimplePlatform.Windows:
                    
                    _clientStream = new NamedPipeClientStream(PipeName);
                    break;
                
                case SimplePlatform.Posix:

                    Task.Factory.StartNew(ConnectToPipePosix, this);
                    break;
                
                default:
                    
                    throw new NotImplementedException();

            }
            
            _clientReader = new StreamReader(_clientStream);

        }

        private void WindowsUpdate()
        {
            
            if (!_connected && !SystemUtility.TryConnectPipeClientWindows((NamedPipeClientStream) _clientStream, out _connected))
            {
                
                return;  // we aren't connected and we can't connect
                
            }
            
            if (_justStarted || _currentGetEventsTask.IsCompleted || _currentGetEventsTask.IsCanceled)
            {
                
                _getEventsCancellationTokenSource = new CancellationTokenSource();
                _currentGetEventsTask = Task.Factory.StartNew(
                    GetEventsActionWindows,
                    this, _getEventsCancellationTokenSource.Token);

            }

            if (_justStarted || _currentVerifyConnectionTask.IsCompleted)
            {

                _currentVerifyConnectionTask = Task.Factory.StartNew(
                    VerifyConnectionActionWindows,
                    this);

            }

            _justStarted = false;
            
        }

        private void PosixUpdate()
        {

            if (!_connected)
                return;

            if (_justStarted || _currentGetEventsTask.IsCompleted)
                _currentGetEventsTask = Task.Factory.StartNew(GetEventsActionPosix, this);

            _justStarted = false;

        }

        private void ExecuteEvents()
        {
            lock (_eventListLock)
            {
                
                while (_eventList.Count > 0)
                {

                    var command = _eventList[0].Split(' ');
                    _eventList.RemoveAt(0);
                    ExecuteCommand(command);

                }
                
            }

        }

        private void ExecuteCommand(IEnumerable<string> args)
        {

            var argsList = new List<string>(args);
            var keyword = argsList[0];
            argsList.RemoveAt(0);
            switch (keyword)
            {
                
                case "SET":

                    Set(argsList);
                    break;

            }

        }

        private void Set(List<string> remainingCommand)
        {
            
            switch (remainingCommand[0])
            {
                
                case "tire":
                    
                    _actionHandler.SetTireTorque(remainingCommand[1], (int) double.Parse(remainingCommand[2]));
                    break;
                
                case "steering":
                    
                    _actionHandler.SetTireSteering(remainingCommand[1], (int) double.Parse(remainingCommand[2]));
                    break;

            }
            
        }

    }

}
