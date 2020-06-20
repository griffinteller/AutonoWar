using System.Collections.Generic;
using ExitGames.Client.Photon;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class ServerConnector : MonoBehaviourPunCallbacks
    {
        public const string NickNameKey = "NickName";
        private static readonly string[] RoomPropertiesForLobby = {"gameMode", "map"};

        private readonly Dictionary<Player, GameObject> _playerListItems = new Dictionary<Player, GameObject>();

        private bool _creatingServer;

        private Hashtable _serverDescription;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject playerListItemPrefab;
        [SerializeField] private RectTransform playerListParent;
        [SerializeField] private CanvasGroup roomWaitingPanel;

        [SerializeField] private ServerSettingsHandler serverSettingsHandler;
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject topBar;
        [SerializeField] private UiUtility uiUtility;
        [SerializeField] private Text waitingText;

        public void StartServer()
        {
            _creatingServer = true;

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = Application.version;
                PhotonNetwork.ConnectUsingSettings();
                return;
            }

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
                return;
            }

            _serverDescription = serverSettingsHandler.GetServerDescription();
            PhotonNetwork.NickName = PlayerPrefs.GetString(NickNameKey);
            CreateRoomUsingDescription();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (!newMasterClient.Equals(PhotonNetwork.LocalPlayer))
                return;

            waitingText.text = "Waiting for players...";
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(StartGame);
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            if (!_creatingServer)
                return;

            StartServer();
        }

        private void CreateRoomUsingDescription()
        {
            PhotonNetwork.CreateRoom((string) _serverDescription["name"], new RoomOptions
            {
                MaxPlayers = (byte) _serverDescription["maxPlayers"],
                CustomRoomProperties = _serverDescription,
                CustomRoomPropertiesForLobby = RoomPropertiesForLobby,
                IsOpen = true,
                IsVisible = true
            });
        }

        public void JoinServer(string serverName)
        {
            _creatingServer = false;

            if (!PhotonNetwork.IsConnected || !PhotonNetwork.InLobby)
                return;

            PhotonNetwork.NickName = PlayerPrefs.GetString(NickNameKey);
            PhotonNetwork.JoinRoom(serverName);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            _creatingServer = false;
            uiUtility.RaiseError("Could not create server:\n" + message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            uiUtility.RaiseError("Could not join server:\n" + message);
        }

        private void AddAllCurrentPlayersToList()
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                AddPlayerToList(player);
        }

        public override void OnJoinedRoom() // this is called when creating room too
        {
            _creatingServer = false;
            Debug.Log(PhotonNetwork.CurrentRoom.Name);

            uiUtility.SwitchToPanel(roomWaitingPanel);
            topBar.SetActive(false);

            AddAllCurrentPlayersToList();

            PhotonNetwork.AutomaticallySyncScene = true;

            if (PhotonNetwork.IsMasterClient)
            {
                waitingText.text = "Waiting for players...";
                startButton.gameObject.SetActive(true);
                startButton.onClick.AddListener(StartGame);
            }
            else
            {
                waitingText.text = "Waiting for host...";
                startButton.gameObject.SetActive(false);
            }
        }

        public void StartGame()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(MapEnumWrapper.MapSceneNames[(MapEnum) _serverDescription["map"]]);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayerToList(newPlayer);
        }

        private void AddPlayerToList(Player player)
        {
            var item = Instantiate(playerListItemPrefab, playerListParent);
            item.GetComponentInChildren<Text>().text = player.NickName;
            _playerListItems.Add(player, item);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (otherPlayer.Equals(PhotonNetwork.LocalPlayer))
                return;

            Destroy(_playerListItems[otherPlayer]);
            _playerListItems.Remove(otherPlayer);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.Disconnect();
            startButton.onClick.RemoveListener(StartGame);

            foreach (var obj in _playerListItems.Values)
                Destroy(obj);

            _playerListItems.Clear();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}