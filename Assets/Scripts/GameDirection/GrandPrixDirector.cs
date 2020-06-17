using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using Utility;

using Random = System.Random;

namespace GameDirection
{
    public class GrandPrixDirector : GameDirector
    {
        public override GameModeEnum GameMode => GameModeEnum.GrandPrix;
        public override Dictionary<MapEnum, Vector3> BaseStartingPositions => new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(-280, 0, -410)},
            {MapEnum.Desert, new Vector3(98, 0, -130)}
        };

        private Dictionary<MapEnum, Vector3> _startingLineUpVectors = new Dictionary<MapEnum, Vector3>
        {
            {MapEnum.Highlands, new Vector3(0, 0, 0)},
            {MapEnum.Highlands, new Vector3(0, 0, 0)},
        };

        private const float Spacing = 5;

        public override Dictionary<int, Vector3> GetStartingPositions()
        {
            var playerArray = NetworkUtility.PlayerArrayByActorNumber();
            var result = new Dictionary<int, Vector3>();
            
            Shuffle(playerArray);
            for(var i = 0; i < playerArray.Length; i++)
            {
                var player = playerArray[i];
                var displacement = 
                    -_startingLineUpVectors[CurrentMap] * Spacing * playerArray.Length / 2f
                    + -_startingLineUpVectors[CurrentMap] * Spacing * i;
                
                result.Add(player.ActorNumber, BaseStartingPositions[CurrentMap] + displacement);
            }

            return result;
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