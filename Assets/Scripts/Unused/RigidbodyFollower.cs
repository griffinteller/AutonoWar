using UnityEngine;

namespace Unused
{
    public class RigidbodyFollower : MonoBehaviour
    {
        public Rigidbody structuralRigidbody;

        public void Update()
        {
            transform.position = structuralRigidbody.position;
            structuralRigidbody.transform.localPosition = Vector3.zero;
        }
    }
}