using System;
using System.Collections.Generic;
using System.Globalization;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

using Random = System.Random;

namespace GameDirection
{
    public class GrandPrixDirector : DefaultCycleGameDirector
    {
        public override GameModeEnum GameMode => GameModeEnum.GrandPrix;
        public override HashSet<HudElement> HudElements => new HashSet<HudElement>
        {
            HudElement.Clock
        };
        public override Dictionary<MapEnum, Vector3> BaseStartingPositions => new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(862.9f, 88.8f, 1118.2f)},
            {MapEnum.Desert, new Vector3(1143.9f, 83.23f, -808.7f)}
        };

        private readonly Dictionary<MapEnum, Quaternion> _startingLineUpRotations = new Dictionary<MapEnum, Quaternion>
        {
            {MapEnum.Highlands, Quaternion.Euler(0, -152, 0)},
            {MapEnum.Desert, Quaternion.Euler(0, -57, 0)},
        };

        private readonly Dictionary<MapEnum, Vector3> _endpointRanges = new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(1300, 0, 1300)},
            {MapEnum.Desert, new Vector3(1300, 0, 1300)}
        };

        private const float MinimumFlatDistance = 1500;

        [FormerlySerializedAs("endpointObject")] public GameObject startPointObject;
        public GameObject beaconObject;
        public Color beaconColor;
        public GameObject scoreboardPrefab;

        private const float Spacing = 5;

        private UiClock _clock;
        private Scoreboard _scoreboard;
        
        private List<ScoreboardColumn> _columns = new List<ScoreboardColumn>
        {
            new ScoreboardColumn("Rank"),
            new ScoreboardColumn("Name", expand: true),
            new ScoreboardColumn("Distance", isFloat: true),
            new ScoreboardColumn("Time")
        };
        
        public Vector3 Endpoint { get; private set; }

        public override Dictionary<int, PositionRotationPair> GetStartingPositionsAndRotations()
        {
            var playerArray = NetworkUtility.PlayerArrayByActorNumber();
            var result = new Dictionary<int, PositionRotationPair>();
            const float distanceOffGround = 1f;
            
            Shuffle(playerArray);
            for(var i = 0; i < playerArray.Length; i++)
            {
                var resultElement = new PositionRotationPair();
                var player = playerArray[i];
                
                var displacement = 
                    _startingLineUpRotations[CurrentMap] * -Vector3.right * Spacing / 2f * (playerArray.Length - 1)
                    + _startingLineUpRotations[CurrentMap] * Vector3.right * Spacing * i;
                
                var pos = BaseStartingPositions[CurrentMap] + displacement;
                pos.y = TerrainUtility.GetClosestCurrentTerrain(pos).SampleHeight(pos) + distanceOffGround;

                resultElement.position = pos;
                resultElement.rotation = _startingLineUpRotations[CurrentMap];

                result.Add(player.ActorNumber, resultElement);
            }

            return result;
        }

        public void Start()
        {
            _clock = FindObjectOfType<UiClock>();

            Endpoint = GetEndpoint();

            var beacon = Instantiate(beaconObject, Endpoint, Quaternion.identity);

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.InstantiateSceneObject(
                    scoreboardPrefab.name, Vector3.zero, Quaternion.identity,
                    0,
                    new object[] {NetworkUtility.Serialize(new object[] {_columns, "Distance"})});

            Instantiate(startPointObject,
                BaseStartingPositions[CurrentMap],
                _startingLineUpRotations[CurrentMap]);
        }

        private Vector3 GetEndpoint()
        {
            var endpointRange = _endpointRanges[CurrentMap];
            Vector3 endpoint;
            var rng = new Random((int) PhotonNetwork.CurrentRoom.CustomProperties["commonRandomSeed"]);
            do
            {
                endpoint = new Vector3(
                    (float) rng.NextDouble() * 2 * endpointRange.x - endpointRange.x,
                    0,
                    (float) rng.NextDouble() * 2 * endpointRange.z - endpointRange.z);
                endpoint.y = TerrainUtility.GetClosestCurrentTerrain(endpoint).SampleHeight(endpoint);
            } while (Vector3.Distance(
                         BaseStartingPositions[CurrentMap],
                         endpoint)
                     < MinimumFlatDistance);

            return endpoint;
        }

        protected override void PreGameSetup()
        {
            base.PreGameSetup();
            _clock.stopwatch = true;
        }

        protected override void GameStartSetup()
        {
            base.GameStartSetup();
            _clock.StartClock();
        }

        protected override void PreGameUpdate()
        {
            base.PreGameUpdate();
            CheckScoreBoardIsInstantiated();
        }

        protected override void InGameUpdate()
        {
            base.InGameUpdate();
            if (CheckScoreBoardIsInstantiated() && PhotonNetwork.IsMasterClient)
                UpdateScoreboard();
        }

        private void UpdateScoreboard()
        {
            foreach (var pair in PhotonNetwork.CurrentRoom.Players)
            {
                var actorNumber = pair.Key;

                _scoreboard.SetCellByActorNumber(
                    actorNumber,
                    "Distance",
                    Vector3.Distance(
                        playerConnection.robots[actorNumber].transform.position,
                        Endpoint));
            }
        }

        private bool CheckScoreBoardIsInstantiated()
        {
            if (_scoreboard)
                return true;

            _scoreboard = GameObject.FindWithTag("Scoreboard").GetComponent<Scoreboard>();

            if (_scoreboard)
                _scoreboard.reverseRank = true;
            
            return _scoreboard;
        }

        private static void Shuffle<T>(IList<T> list)  
        {
            var rng = new Random((int) PhotonNetwork.CurrentRoom.CustomProperties["commonRandomSeed"]);
            
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}