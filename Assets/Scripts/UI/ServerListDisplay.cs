using System.Collections.Generic;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ServerListDisplay : MonoBehaviourPunCallbacks
    {
        private readonly Dictionary<string, GameObject> _serverListings = new Dictionary<string, GameObject>();

        private bool _even = true;
        [SerializeField] private Color evenColor;


        [SerializeField] private RectTransform listParent;
        [SerializeField] private ServerConnector serverConnector;
        [SerializeField] private ServerListItem serverListItem;

        public void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = Application.version;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            roomList.Sort(RoomAlphaCompare);

            foreach (var roomInfo in roomList)
            {
                if (roomInfo.RemovedFromList || roomInfo.PlayerCount >= roomInfo.MaxPlayers)
                {
                    if (_serverListings.ContainsKey(roomInfo.Name))
                    {
                        Destroy(_serverListings[roomInfo.Name]);
                        _serverListings.Remove(roomInfo.Name);
                        _even = !_even;
                    }

                    continue;
                }

                if (_serverListings.ContainsKey(roomInfo.Name))
                    continue;

                AddServerListing(roomInfo);
            }
        }

        private void AddServerListing(RoomInfo roomInfo)
        {
            var listItem = Instantiate(serverListItem.gameObject, listParent);
            _serverListings.Add(roomInfo.Name, listItem);

            var listItemComponent = listItem.GetComponent<ServerListItem>();

            var mapName = new MapEnumWrapper
            {
                Index = (int) roomInfo.CustomProperties["map"]
            }.ToString();

            var gameModeName = new GameModeEnumWrapper
            {
                Index = (int) roomInfo.CustomProperties["gameMode"]
            }.ToString();

            listItemComponent.serverNameText.text = roomInfo.Name;
            listItemComponent.mapText.text = mapName;
            listItemComponent.playersText.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
            listItemComponent.gameModeText.text = gameModeName;

            listItem.GetComponent<Button>().onClick.AddListener(
                delegate { serverConnector.JoinServer(roomInfo.Name); });

            if (!_even)
                return;

            _even = false;
            listItem.GetComponent<Image>().color = evenColor;
        }

        private static int RoomAlphaCompare(RoomInfo r1, RoomInfo r2)
        {
            return string.Compare(r1.Name, r2.Name);
        }
    }
}