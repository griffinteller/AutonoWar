using UnityEngine;

namespace GamePhysics
{
    public class Motor : MonoBehaviour
    {
        public float torque;
        public float multiplier;

        public GameObject someOtherObject;

        private Rigidbody _rigidbody;
        private ConfigurableJoint _joint;

        public void Awake()
        {
            _rigidbody = someOtherObject.GetComponent<Rigidbody>();
            _joint = GetComponent<ConfigurableJoint>();
        }

        public void FixedUpdate()
        {
            _rigidbody.AddTorque(torque * multiplier * _joint.connectedBody.transform.right);
        }
    }
}