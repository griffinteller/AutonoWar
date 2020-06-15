using UnityEngine;

namespace Unused
{
    public class BetterWheelCollider : MonoBehaviour
    {
        public float contactDistance = 0.05f;
        public LayerMask mask;
        public float radius;
        public int raycastDensity;
        public float torque;

        public void FixedUpdate()
        {
            var t = transform;

            var hitArray = new RaycastHit[raycastDensity];
            var numContacts = 0;

            for (var i = 0; i < raycastDensity; i++)
            {
                var orientation =
                    Quaternion.Euler(i * 360f / raycastDensity, 0, 0)
                    * t.forward;
                var location = t.position + orientation * radius;

                if (!Physics.Raycast(location, orientation,
                    out var hit, contactDistance * 2, mask))
                    continue;

                numContacts += 1;
            }

            var totalForce = Vector3.zero;

            for (var i = 0; i < raycastDensity; i++)
            {
                var hit = hitArray[i];
            }
        }
    }
}