using System;
using System.Collections.Generic;
using Networking;
using Photon.Pun;
using UnityEngine;
using Utility;

namespace GameDirection
{
    public abstract class GameDirector : MonoBehaviour
    {
        public abstract GameModeEnum GameMode { get; }

        public bool FullyLoaded
        {
            get => _fullyLoaded;
            set
            {
                _fullyLoaded = value;
                if (_fullyLoaded)
                    OnFullyLoaded();
            }
        }

        private bool _fullyLoaded;

        protected virtual void OnFullyLoaded() {}

        public virtual Dictionary<int, Vector3> GetStartingPositions(Vector3 center)
        {
            const float radius = 10f;
            const float distanceOffGround = 1f;
            var players = PhotonNetwork.CurrentRoom.Players;

            var result = new Dictionary<int, Vector3>();
            var i = 0;
            foreach (var pair in players)
            {
                var pos = center;
                pos.x += Mathf.Cos(radius * 2 * Mathf.PI / players.Count * i);
                pos.z += Mathf.Sin(radius * 2 * Mathf.PI / players.Count * i);
                pos.y = TerrainUtility.GetClosestCurrentTerrain(center).SampleHeight(center) + distanceOffGround;

                result.Add(pair.Key, pos);
                i++;
            }

            return result;
        }
    }
} 