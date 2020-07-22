using UnityEngine;

namespace GamePhysics
{
    public class TireCollider : MonoBehaviour
    {
        public float mass = 1f;
        public float radius = 0.5f;
        
        [Space(10)]
        
        [Header("Suspension")]
        
        public float suspensionSpring;
        public float suspensionDamper;
        public float suspensionDistance;
        
        [Space(10)]
        
        public int numberOfRaycasts = 10;
        public float groundingDistance = 0.01f;
        public float motorTorque = 0f;
        public float steeringAngle = 0f;

        [Space(10)] [Header("Friction")]

        public FrictionCurve forwardFriction;
        public FrictionCurve sidewaysFriction;

        public float RadPerSec { get; private set; }
        public float Rpm => RadPerSec * 2 * Mathf.PI / 60f;
        public float MomentOfIntertia => 0.5f * mass * radius * radius;
        public float AngularMomentum => MomentOfIntertia * RadPerSec;
        public Vector3 BottomOfTireRelativeVelocity => -RadPerSec * radius * 
                                                       (Quaternion.Euler(0, steeringAngle, 0) * Vector3.forward);

        private RaycastHit[] _raycastHits;
        private Rigidbody _rigidbody;
        private Rigidbody _parentRigidbody;
        private ConfigurableJoint _joint;
        
        private float _degreesBetweenRaycasts;
        private float _range;

        public void Awake()
        {
            _degreesBetweenRaycasts = 360f / numberOfRaycasts;
            _range = radius + groundingDistance;
            _raycastHits = new RaycastHit[numberOfRaycasts];

            transform.localPosition += Vector3.down * suspensionDistance;
        }

        public void Start()
        {
            ConfigureRigidBody();
            ConfigureJoint();
        }

        private void ConfigureRigidBody()
        {
            _parentRigidbody = GetComponentInParent<Rigidbody>();
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.mass = mass;
            _rigidbody.angularDrag = 0;
        }

        private void ConfigureJoint()
        {
            _joint = gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = _parentRigidbody;
            _joint.axis = Vector3.right;
            _joint.secondaryAxis = Vector3.up;
            
            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Free;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            
            _joint.angularXMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Locked;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;

            //_joint.projectionMode = JointProjectionMode.PositionAndRotation;
            _joint.enablePreprocessing = false;
            _joint.projectionMode = JointProjectionMode.PositionAndRotation;

            var drive = _joint.yDrive;
            drive.positionSpring = suspensionSpring;
            drive.positionDamper = suspensionDamper;
            _joint.yDrive = drive;
        }
        
        public void FixedUpdate()
        {
            ApplyTorque(motorTorque);
            
            var numberOfHits = PopulateHitArray();
            ApplyForces(numberOfHits);
        }

        private void ApplyForces(int numberOfHits)
        {
            var relativeVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
            var slipVector = relativeVelocity - (-BottomOfTireRelativeVelocity);
            slipVector = Vector3.ProjectOnPlane(slipVector, Vector3.up); // we don't care about moving up or down
            // this is captured in the spring of the joint

            var planeVelocity = Vector3.ProjectOnPlane(relativeVelocity, Vector3.up);

            var forwardSlipProportion = slipVector.z / Mathf.Abs(planeVelocity.z);
            var sidewaysSlipPropotion = slipVector.x / Mathf.Abs(planeVelocity.x);

            var forwardCoefficient = forwardFriction.GetCoefficientAt(Mathf.Abs(forwardSlipProportion));
            var sidewaysCoefficient = sidewaysFriction.GetCoefficientAt(Mathf.Abs(sidewaysSlipPropotion));
            
            print(name + ": " + forwardCoefficient);

            var sprungWeight = _joint.currentForce.magnitude;

            var forwardForce = -Mathf.Sign(slipVector.z) * forwardCoefficient * sprungWeight;
            ApplyTorque(forwardForce * radius);

            var sidewaysForce = -Mathf.Sign(slipVector.x) * sidewaysCoefficient * sprungWeight;
            _rigidbody.AddForce(sidewaysForce * transform.right);

            for (var i = 0; i < numberOfHits; i++)
            {
                var hit = _raycastHits[i];
                var normal = hit.normal;
                var projectedNormal = Vector3.ProjectOnPlane(normal, transform.right);
                var worldForceDirection = Vector3.Cross(Vector3.right, projectedNormal).normalized;
                
                _rigidbody.AddForce(forwardForce * worldForceDirection / numberOfHits);
            }
        }

        private void ApplyTorque(float torque)
        {
            RadPerSec += torque / MomentOfIntertia * Time.fixedDeltaTime;
        }

        private void ApplyForcesToBody(float numberOfHits)
        {
            var t = transform;
            var right = t.right;
            var up = t.up;
            
            for (var i = 0; i < numberOfHits; i++)
            {
                var hit = _raycastHits[i];
                var forceDirection = 
                    Vector3.Cross(right, Vector3.Project(hit.normal, up)).normalized;
                var force = forceDirection * motorTorque / hit.distance / numberOfHits;
                _parentRigidbody.AddForceAtPosition(force, t.position);
            }
            
            _parentRigidbody.AddTorque(motorTorque * -right);
        }

        private int PopulateHitArray()
        {
            var numberOfHits = 0;
            for (var i = 0; i < numberOfRaycasts; i++)
            {
                var direction = Quaternion.AngleAxis(i * _degreesBetweenRaycasts, transform.right) 
                                * Vector3.down;
                
                if (Physics.Raycast(transform.position, direction, out var hit, _range))
                {
                    _raycastHits[numberOfHits] = hit;
                    numberOfHits++;
                }
            }

            return numberOfHits;
        }

    }
}