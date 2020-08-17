using UnityEngine;

namespace GamePhysics.Tire
{
    public class TireColliderFollower : MonoBehaviour
    {
        public TireCollider parentCollider;

        public void Start()
        {
            parentCollider = GetComponentInParent<TireCollider>();
        }

        public void FixedUpdate()
        {
            var t       = transform;
            var parentT = t.parent;
            transform.position = parentT.position + (parentCollider.Length - parentCollider.CompressionDistance) * -parentT.up;
        }
    }
}
