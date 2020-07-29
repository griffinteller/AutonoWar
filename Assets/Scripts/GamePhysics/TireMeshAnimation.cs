using System;
using UnityEngine;

namespace GamePhysics
{
    public class TireMeshAnimation : MonoBehaviour
    {
        public TireCollider parentCollider;

        public void Update()
        {
            transform.Rotate(transform.right, parentCollider.radPerSec * 180f / Mathf.PI * Time.deltaTime, 
                Space.World);
        }
    }
}