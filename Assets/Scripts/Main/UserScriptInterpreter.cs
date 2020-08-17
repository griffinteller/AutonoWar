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
        private const char Separator                     = ';';
        private const byte MaxConnectionAttemptsPerFrame = 5;

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
            var stillConnected =
                SystemUtility.IsDuplexPipeStillConnectedWindows((NamedPipeClientStream) usiCast._clientStream);

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

            var tries = 0;
            while (true)
            {
                if (tries == MaxConnectionAttemptsPerFrame)
                {
                    usiCast._connected = false;
                    
                    #if UNITY_EDITOR
                    Debug.Log("Usi connection giving up until next frame.");
                    #endif
                    
                    return;
                }
                
                try
                {
                    usiCast._clientStream = new FileStream("/tmp/" + usiCast.PipeName, FileMode.Open, FileAccess.Read);
                    break;
                }
                catch (IOException e)
                {
                    tries++;
                }
            }

            usiCast._clientReader = new StreamReader(usiCast._clientStream);

            usiCast._connected = true;
            Debug.Log("USI Connected!");
        };

        private readonly object _clientStreamLock = new object();
        private readonly List<string> _eventList = new List<string>();

        private readonly object _eventListLock = new object();

        private ActionHandler _actionHandler;
        private StreamReader _clientReader;
        private Stream _clientStream;

        private bool                    _connected;
        private Task                    _currentGetEventsTask;
        private Task                    _currentVerifyConnectionTask;
        private Task                    _currentPosixConnectionTask;
        private CancellationTokenSource _getEventsCancellationTokenSource;
        private bool                    _justStarted = true;
        private SimplePlatform          _platform;
        private RobotMain               _robotMain;

        private string PipeName = "EventQueuePipe";

        public void Start()
        {
            _platform = SystemUtility.GetSimplePlatform();
            _actionHandler = GetComponent<ActionHandler>();
            _robotMain = GetComponent<RobotMain>();
            PipeName += _robotMain.robotIndex;

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
                    _clientReader = new StreamReader(_clientStream);
                    break;

                case SimplePlatform.Posix:

                    _currentPosixConnectionTask = Task.Factory.StartNew(ConnectToPipePosix, this);
                    break;

                default:

                    throw new NotImplementedException();
            }
        }

        private void WindowsUpdate()
        {
            if (!_connected &&
                !SystemUtility.TryConnectPipeClientWindows((NamedPipeClientStream) _clientStream, out _connected))
                return; // we aren't connected and we can't connect

            if (_justStarted || _currentGetEventsTask.IsCompleted || _currentGetEventsTask.IsCanceled)
            {
                _getEventsCancellationTokenSource = new CancellationTokenSource();
                _currentGetEventsTask = Task.Factory.StartNew(
                    GetEventsActionWindows,
                    this, _getEventsCancellationTokenSource.Token);
            }

            if (_justStarted || _currentVerifyConnectionTask.IsCompleted)
                _currentVerifyConnectionTask = Task.Factory.StartNew(
                    VerifyConnectionActionWindows,
                    this);

            _justStarted = false;
        }

        private void PosixUpdate()
        {
            if (!_connected)
            {
                if (_currentGetEventsTask == null 
                 || _currentPosixConnectionTask.IsCompleted 
                 || _currentGetEventsTask.IsFaulted)
                    InitStreams();
                else
                    return;
            }

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

            if (!char.IsLetter(keyword[0]))
                keyword = keyword.Substring(1);

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
            var keyword = remainingCommand[0];
            remainingCommand.RemoveAt(0);
            
            switch (keyword)
            {
                case "tire":

                    //_actionHandler.SetTirePower(remainingCommand[1], (float) double.Parse(remainingCommand[2]));
                    Tire(remainingCommand);
                    break;
            }
        }

        private void Tire(List<string> remainingCommand)
        {
            var keyword = remainingCommand[0];
            remainingCommand.RemoveAt(0);

            try
            {
                switch (keyword)
                {
                    case "power":

                        _actionHandler.SetTirePower(remainingCommand[0], (float) double.Parse(remainingCommand[1]));
                        break;

                    case "steering":

                        _actionHandler.SetTireSteering(remainingCommand[0], (float) double.Parse(remainingCommand[1]));
                        break;

                    case "brake":

                        _actionHandler.SetTireBrake(remainingCommand[0], (float) double.Parse(remainingCommand[1]));
                        break;
                }
            }
            catch (FormatException e)
            {
                Debug.LogError("Format exception! String was: " + remainingCommand[1]);
            }
        }
    }
}