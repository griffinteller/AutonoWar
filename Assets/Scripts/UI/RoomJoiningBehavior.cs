using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class RoomJoiningBehavior : MonoBehaviourPunCallbacks
    {
    
        public byte minPlayersPerMatch = 1;
        public byte maxPlayersPerMatch = 10;

        public string freeplayScene;
        public string classicTagScene;

        public MainMenuHandler mainMenuHandler;

        public ExitGames.Client.Photon.Hashtable roomProperties = 
            new ExitGames.Client.Photon.Hashtable {{"Gamemode", "Freeplay"}};
        
        public Text playerList;

        public Button startButton;
        
        private string _gameVersion;

        private bool _findingRoom = false;

        private string[] roomPropertyList = {"Gamemode"};
        
        private const string PlayerListStartString = "Connected Players:\n";


        public void SetGamemode(string gamemode)
        {

            roomProperties["Gamemode"] = gamemode;

        }

        public void SetGamemode(Dropdown dropdown)
        {
            
            SetGamemode(dropdown.options[dropdown.value].text);
            
        }

        public void Awake()
        {

            _gameVersion = Application.version;
            PhotonNetwork.AutomaticallySyncScene = true;

        }

        public void FindMatch()
        {

            Debug.Log("Finding Match");
            _findingRoom = true;
            PhotonNetwork.NickName = PlayerPrefs.GetString(MultiPlayerMenuScript.NickNameKey);
            PhotonNetwork.GameVersion = _gameVersion;
            if (PhotonNetwork.IsConnected)
            {

                PhotonNetwork.JoinRandomRoom(roomProperties, maxPlayersPerMatch);

            }
            else
            {

                PhotonNetwork.GameVersion = _gameVersion;
                PhotonNetwork.ConnectUsingSettings();

            }
        
        }

        public override void OnConnectedToMaster()
        {
            
            Debug.Log("Connected to Photon. Joining Room.");
            if (_findingRoom)
            {

                PhotonNetwork.JoinRandomRoom(roomProperties, maxPlayersPerMatch);

            }

        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            
            Debug.Log($"Disconnected due to: {cause}");
            mainMenuHandler.DisplayHomePanel();
            
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            
            Debug.Log("No rooms found, creating a new room.");
            Debug.Log(roomProperties);
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = maxPlayersPerMatch, 
                CustomRoomProperties = roomProperties,
                CustomRoomPropertiesForLobby = roomPropertyList,
                IsOpen = true,
                IsVisible = true

            });

        }

        public override void OnJoinedRoom()
        {
            
            Debug.Log("Successfully joined room.");
            var currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            DisplayNames();
            if (currentPlayers < minPlayersPerMatch)
            {

                return;

            }

            if (currentPlayers < maxPlayersPerMatch)
            {

                UpdateStartButtonInteractable();

            }
            else
            {

                StartMatch();

            }
            
        }

        public void StartMatch()
        {
            
            Debug.Log("Starting Match!");
            PhotonNetwork.CurrentRoom.IsOpen = false;
            switch (PhotonNetwork.CurrentRoom.CustomProperties["Gamemode"])
            {
                
                case "Freeplay":

                    SceneManager.LoadScene(freeplayScene);
                    break;
                
                case "Classic Tag":

                    SceneManager.LoadScene(classicTagScene);
                    break;
                
            }
            
        }

        public void DisplayNames()
        {

            playerList.text = PlayerListStartString;
            foreach (var pair in PhotonNetwork.CurrentRoom.Players)
            {

                var playerName = pair.Value.NickName;
                playerList.text += "\n -" + playerName;
                
            }
            
            
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            
            DisplayNames();
            UpdateStartButtonInteractable();
            
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            
            DisplayNames();
            UpdateStartButtonInteractable();
            
        }

        private void UpdateStartButtonInteractable()
        {

            if (PhotonNetwork.IsMasterClient)
            {
                
                startButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersPerMatch;
                
            }

        }
        
    }
}
