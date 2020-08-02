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
        
        public float radPerSec;
        public float Rpm => radPerSec / 2 / Mathf.PI / 60f;
        public float MomentOfIntertia => 0.5f * mass * radius * radius;
        public float AngularMomentum => MomentOfIntertia * radPerSec;
        public Vector3 BottomOfTireRelativeVelocity => -radPerSec * radius * 
                                                       (Quaternion.Euler(0, steeringAngle, 0) * Vector3.forward);
        public float SpringExtension => Vector3.Dot(transform.localPosition - _joint.connectedAnchor, transform.up);
        public float SprungForce => SpringExtension * suspensionSpring;
        
        private RaycastHit[] _raycastHits;
        private Rigidbody _rigidbody;
        private Rigidbody _parentRigidbody;
        private ConfigurableJoint _joint;
        private SphereCollider _collider;
        
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
            ConfigureCollider();
            ConfigureRigidbody();
            ConfigureJoint();
            ConfigureMesh();
        }

        private void ConfigureMesh()
        {
            var child = transform.GetChild(0);
            if (!child)
            {
                Debug.LogWarning("Tire Collider has no child mesh.");
                return;
            }

            var childAnimator = child.gameObject.AddComponent<TireMeshAnimation>();
            childAnimator.parentCollider = this;
        }

        private void ConfigureCollider()
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.radius = radius;
            _collider.material = new PhysicMaterial();

            var mat = _collider.material;
            mat.dynamicFriction = 0f;
            mat.staticFriction = 0f;
            mat.frictionCombine = PhysicMaterialCombine.Minimum;
        }

        private void ConfigureRigidbody()
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
            var numberOfHits = PopulateHitArray();
        }
        

        private void ApplyForces(int numberOfHits)
        {
            if (numberOfHits == 0)
                return;

            var t = transform;
            var right = t.right;

            var hit = _raycastHits[0];
            var normal = hit.normal;
            var hitPoint = hit.point;
            var rightOnSurface = Vector3.ProjectOnPlane(right, normal).normalized;
            var forwardOnSurface = Vector3.Cross(rightOnSurface, normal);

            var velocity = _rigidbody.velocity;
            var contactPointEdgeVelocity = radPerSec * radius * -forwardOnSurface;
            
            var forwardVelocityOnSurface = Vector3.Project(velocity, forwardOnSurface);
            var sidewaysSlip = Vector3.Dot(velocity, rightOnSurface);
            var forwardsSlip = Vector3.Dot(forwardVelocityOnSurface + contactPointEdgeVelocity, forwardOnSurface);

            var sprungWeight = (_parentRigidbody.mass / 4 + mass) * Physics.gravity.magnitude;

            var forwardForce = -Mathf.Sign(forwardsSlip) * sprungWeight 
                                                        * forwardFriction.GetCoefficientAt(forwardsSlip) * forwardOnSurface;
            var sidewaysForce = -Mathf.Sign(sidewaysSlip) * sprungWeight 
                                                          * sidewaysFriction.GetCoefficientAt(sidewaysSlip) * rightOnSurface;
            
            //Debug.Log(forwardForce);
            _parentRigidbody.AddForceAtPosition(forwardForce + sidewaysForce, t.position);
            ApplyTorque(Mathf.Sign(forwardsSlip) * forwardForce.magnitude / hit.distance);


            /*var relativeVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
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
            }*/
        }

        private void ApplyTorque(float torque)
        {
            var delta =  torque / MomentOfIntertia * Time.fixedDeltaTime;
            radPerSec += delta;
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