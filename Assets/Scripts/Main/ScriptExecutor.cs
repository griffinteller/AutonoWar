using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Main
{
    public class ScriptExecutor : MonoBehaviour
    {

        public string scriptName;
        private Process _process;

        private ActionHandler _actionHandler;
        
        private string _scriptPath;
        
        public void Start()
        {

            _actionHandler = GameObject.Find("Robot").GetComponent<ActionHandler>();
            _scriptPath = _actionHandler.dataDirectory + scriptName;

        }

        private void Execute()
        {
            
            _process = new Process();
            _process.StartInfo.FileName = @"cmd.exe";
            _process.StartInfo.Arguments = "/c python " + _scriptPath;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.Start();

        }

        private void StopExecution()
        {

            _process.CloseMainWindow();
            _process = null;

        }

        public void ToggleExecution()
        {
            
            if (_process == null)
            {


                GetComponentInChildren<Text>().text = "Stop Script";
                Execute();
                
            }
            else
            {
                
                GetComponentInChildren<Text>().text = "Execute Script";
                StopExecution();
                //_actionHandler.ResetInternalState();
                
            }
            
        }

    }
    
}

