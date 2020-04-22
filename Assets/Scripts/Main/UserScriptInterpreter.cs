using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Main
{

    public class UserScriptInterpreter : MonoBehaviour
    {

        public int maxFileSize;

        private List<string> _eventList = new List<string>();

        private ActionHandler _actionHandler;

        private string _eventQueuePath = "";

        private int _numEventsInQueue; // = 0
        

        public void Start()
        {
            
            _actionHandler = GetComponent<ActionHandler>();
            while (_eventQueuePath.Equals(""))
            {

                _eventQueuePath = _actionHandler.eventQueuePath;

            }
            
            while (true)
            {

                try
                {
                    
                    var queueFile = new FileStream(_eventQueuePath, FileMode.Create, FileAccess.Write);
                    queueFile.Close();
                    break;

                }
                catch (IOException)
                {
                    
                    // continue
                    
                }
                
            }

        }

        public void Update()
        {

            while (true)
            {

                try
                {

                    var queueFile = new FileStream(_eventQueuePath, FileMode.Open, FileAccess.ReadWrite);
                    var queueReader = new StreamReader(queueFile);

                    for (int i = 0; i < _numEventsInQueue; i++)
                    {

                        queueReader.ReadLine();

                    }
                    
                    var line = queueReader.ReadLine();
                    while (line != null)
                    {

                        _numEventsInQueue += 1;
                        _eventList.Add(line);
                        line = queueReader.ReadLine();

                    }

                    if (_numEventsInQueue > maxFileSize)
                    {

                        TruncateFile(queueFile);

                    }

                    queueReader.Close();
                    break;

                }
                catch (IOException)
                {
                    
                    // continue, but it's redundant to actually write it
                    
                }

            }

            ExecuteEvents();

        }

        private void TruncateFile(FileStream file)
        {
            
            _numEventsInQueue = 0;
            file.SetLength(0);
            
        }

        private void ExecuteEvents()
        {

            while (_eventList.Count > 0)
            {

                var command = _eventList[0].Split(' ');
                _eventList.RemoveAt(0);
                ExecuteCommand(command);

            }
            
        }

        private void ExecuteCommand(string[] args)
        {

            var argsList = new List<string>(args);
            var keyword = argsList[0];
            argsList.RemoveAt(0);
            switch (keyword)
            {
                
                case "SET":

                    Set(argsList);
                    break;
                
                case "COORDINATES":

                    Coordinates(argsList);
                    break;
                
            }

        }

        private void Coordinates(List<string> remainingCommand)
        {

            switch (remainingCommand[0])
            {
                
                case "flip":
                    
                    _actionHandler.FlipUserCoordinates();
                    break;
                
            }

        }

        private void Set(List<string> remainingCommand)
        {
            
            switch (remainingCommand[0])
            {
                
                case "tire":
                    
                    _actionHandler.SetTireTorque(remainingCommand[1], float.Parse(remainingCommand[2]));
                    break;
                
                case "steering":
                    
                    _actionHandler.SetTireSteering(remainingCommand[1], float.Parse(remainingCommand[2]));
                    break;

            }
            
        }
        
    }

}
