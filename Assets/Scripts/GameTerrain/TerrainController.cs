using System;
using UnityEngine;

namespace GameTerrain
{
    public class TerrainController : MonoBehaviour
    {
        [SerializeField] private TriangleCache triangleCache;

        public TriangleCache TriangleCache
        {
            get => triangleCache;
            protected set => triangleCache = value;
        }
    }
}