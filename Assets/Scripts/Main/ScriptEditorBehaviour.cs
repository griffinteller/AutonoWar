using UnityEngine;

namespace Main
{
    public class ScriptEditorBehaviour : MonoBehaviour
    {

        /*public string scriptName;

        private ActionHandler _actionHandler;

        private string _scriptPath;
    
        // Start is called before the first frame update
        public void Start()
        {

            _actionHandler = GameObject.Find("Robot").GetComponent<ActionHandler>();
            _scriptPath = _actionHandler.dataDirectory + scriptName;
            while (true)
            {
            
                try
                {

                    var reader = new StreamReader(new FileStream(_scriptPath, FileMode.OpenOrCreate, FileAccess.Read));
                    GetComponent<InputField>().text = reader.ReadToEnd();
                    reader.Close();
                    break;

                }
                catch (IOException)
                {

                    //continue

                }
            
            }

        }

        public void WriteScript()
        {

            while (true)
            {
            
                try
                {

                    var writer = new StreamWriter(new FileStream(_scriptPath, FileMode.Create, FileAccess.Write));
                    writer.Write(GetComponent<InputField>().text);
                    writer.Close();
                    break;

                }
                catch (IOException)
                {

                    //continue

                }
            
            }
        
        
        
        }*/
    
    }
}


