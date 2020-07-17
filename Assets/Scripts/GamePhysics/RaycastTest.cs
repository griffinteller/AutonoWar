using System;
using UnityEngine;

namespace GamePhysics
{
    public class RaycastTest : MonoBehaviour
    {
        public void Update()
        {
            Physics.Raycast(transform.position, Vector3.up, out var hit, 1000);
            print(hit.point);
        }
    }
}