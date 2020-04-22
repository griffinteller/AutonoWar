using System.IO;
using Networking;
using Photon.Pun;
using UnityEngine;

namespace Main
{
    public class RobotStateSender : MonoBehaviour
    {
        
        private string _robotStateFilepath = "";
        
        public Robot robotStateDescription;

        public void Start()
        {

            var actionHandler = GetComponent<ActionHandler>();
            while (_robotStateFilepath.Equals(""))
            {

                _robotStateFilepath = actionHandler.robotStatePath;

            }

            string gamemode;
            int actorNumber = -1;
            ClassicTagScript classicTagScript = null;
            
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
            else
            {

                gamemode = "Singleplayer";

            }

            robotStateDescription = new Robot(transform.Find("Body").gameObject, gamemode, actorNumber, classicTagScript);
            
        }

        // Update is called once per frame
        public void Update()
        {

            robotStateDescription.Update();
            while (true)
            {

                try
                {

                    var fileWriter = new StreamWriter(
                        new FileStream(_robotStateFilepath, FileMode.Create, FileAccess.Write));

                    WriteState(fileWriter);
                    fileWriter.Close();
                    break;

                }
                catch (IOException e)
                {
                    
                    // continue

                }

            }

        }

        private void WriteState(StreamWriter writer)
        {
            
            writer.Write(JsonUtility.ToJson(robotStateDescription));

        }

    }
}
