using System;
using UnityEngine;

namespace GameTerrain
{
    public class TerrainController : MonoBehaviour
    {
        [SerializeField] protected TriangleCache triangleCache;
        [SerializeField] protected Material material;

        public TriangleCache TriangleCache
        {
            get => triangleCache;
            protected set => triangleCache = value;
        }
    }
}