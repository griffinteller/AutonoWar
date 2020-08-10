using GamePhysics.Tire;
using Photon.Pun;
using UnityEngine;

namespace GameDebug
{
    public class TireComponentDebugger : MonoBehaviour
    {
        public float power;
        public float brakeTorque;
        public float steeringAngle;
        public bool  invertPower;
        public float timeMult;

        [Space(10)] public float actualPower;
        public             float motorTorque;
        public             float actualBrakeTorque;
        public             float actualSteeringAngle;
        public             float angularVelocity;

        private TireComponent   _tireComponent;
        private SingleTireMotor _motor;

        public void Awake()
        {
            _tireComponent = GetComponent<TireComponent>();
            _motor         = GetComponent<SingleTireMotor>();
        }

        public void Update()
        {
            Time.timeScale     = timeMult;
            _motor.Power       = invertPower? -power : power;
            _motor.BrakeTorque = brakeTorque;

            actualPower         = _motor.Power;
            motorTorque         = _motor.MotorTorque;
            actualBrakeTorque   = _motor.BrakeTorque;
            actualSteeringAngle = _tireComponent.SteeringAngle;
            angularVelocity     = _tireComponent.AngularVelocity;
        }
    }
}
