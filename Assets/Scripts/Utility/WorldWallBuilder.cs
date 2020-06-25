using System;
using Networking;
using UnityEngine;

namespace Utility
{
    [ExecuteInEditMode]
    public class WorldWallBuilder : MonoBehaviour
    {
        public float depth;
        public float height;
        public float inset;
        public MapEnum map;

        private Vector3 _size;

        public void Update()
        {
            _size = MapEnumWrapper.MapSizes[map];
            _size -= Vector3.one * inset;
        }

        public void BuildWalls()
        {
            MetaUtility.DestroyImmediateAllChildren(gameObject);
            
            var walls = new BoxCollider[4];
            for (var i = 0; i < 4; i++)
            {
                var wall = Instantiate(
                    new GameObject("Wall" + i, typeof(BoxCollider)), transform);
                walls[i] = wall.GetComponent<BoxCollider>();

                var center = new Vector3();
                center.x = (-Mathf.Abs(i - 1) + 1) * _size.x / 2;
                center.z = (Mathf.Abs(i - 2) - 1) * _size.z / 2;
                walls[i].center = center;
                walls[i].size = new Vector3(
                    Mathf.Abs(center.x) < 1 ? _size.x : depth, 
                    height,
                    Mathf.Abs(center.z) < 1 ? _size.z : depth);
            }
        }
    }
}