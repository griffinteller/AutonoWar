using System;
using System.Collections.Generic;
using Networking;
using Photon.Pun;
using UI;
using UnityEngine;
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
            {MapEnum.Highlands, new Vector3(872, 0, 1130)},
            {MapEnum.Desert, new Vector3(1186, 0, -830)}
        };

        private readonly Dictionary<MapEnum, Vector3> _startingLineUpVectors = new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, Quaternion.Euler(0, 43, 0) * Vector3.one},
            {MapEnum.Desert, Quaternion.Euler(0, -66, 0) * Vector3.one},
        };

        private const float Spacing = 5;

        private UiClock _clock;

        public void Awake()
        {
            _clock = FindObjectOfType<UiClock>();
        }

        public override Dictionary<int, Vector3> GetStartingPositions()
        {
            var playerArray = NetworkUtility.PlayerArrayByActorNumber();
            var result = new Dictionary<int, Vector3>();
            const float distanceOffGround = 1f;
            
            Shuffle(playerArray);
            for(var i = 0; i < playerArray.Length; i++)
            {
                var player = playerArray[i];
                var displacement = 
                    -_startingLineUpVectors[CurrentMap] * Spacing * playerArray.Length / 2f
                    + -_startingLineUpVectors[CurrentMap] * Spacing * i;

                var pos = BaseStartingPositions[CurrentMap] + displacement;
                pos.y = TerrainUtility.GetClosestCurrentTerrain(pos).SampleHeight(pos) + distanceOffGround;
                result.Add(player.ActorNumber, pos);
            }

            return result;
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

        private static Random rng = new Random();  

        private static void Shuffle<T>(IList<T> list)  
        {  
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