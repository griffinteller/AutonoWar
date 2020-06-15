using UnityEngine;

namespace Unused
{
    public class TireMotor : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        public bool invertTorque;
        public float torque;

        public void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void FixedUpdate()
        {
            var trueTorque = torque;

            if (invertTorque)
                trueTorque *= -1;

            _rigidbody.AddRelativeTorque(trueTorque, 0, 0, ForceMode.Force);
        }
    }
}