using UnityEngine;

namespace GamePhysics.Tire
{
    public class TireMeshAnimation : MonoBehaviour
    {
        [HideInInspector] public TireCollider parentCollider;

        private float _degreesAround;

        public void Start()
        {
            parentCollider = GetComponentInParent<TireCollider>();
        }

        public void Update()
        {
            var t       = transform;
            var parentT = t.parent;
            t.position = parentT.position + (parentCollider.Length - parentCollider.CompressionDistance) * -parentT.up;

            _degreesAround += parentCollider.AngularVelocity * 180f / Mathf.PI * Time.deltaTime;
            t.rotation = parentT.rotation
                       * Quaternion.AngleAxis(parentCollider.SteeringAngle, parentT.up)
                       * Quaternion.AngleAxis(_degreesAround, Vector3.right);
        }
    }
}