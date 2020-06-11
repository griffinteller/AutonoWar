using Main;
using Networking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class InGameUIScript : MonoBehaviour
    {

        public PlayerConnection playerConnection;
        public GameObject singlePlayerRobot;
        
        public void ReturnToMainMenu()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
            SceneManager.LoadScene("MainMenuScene");
        }
        
        public void ResetRobot()
        {
            if (playerConnection)
            {
                playerConnection.playerObject.GetComponent<ActionHandler>().ResetRobot();
            }
            else
            {
                singlePlayerRobot.GetComponent<ActionHandler>().ResetRobot();
            }
        }
    }
}
