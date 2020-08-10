using System;
using UnityEngine;

namespace GamePhysics.Tire
{
    public class TireCollider : MonoBehaviour
    {
        #region REGION - Private variables

        //private vars with either external get/set methods, or internal calculated fields from other fields get/set methods
        private GameObject _wheel;
        private float      _wheelMass   = 1f;
        private float      _wheelRadius = 0.5f;
        private float      _suspensionForceOffset;
        private float      _currentBrakeTorque;

        private TireFrictionCurve _fwdFrictionCurve = new TireFrictionCurve(
                0.06f, 1.2f, 0.065f, 1.25f, 0.7f
                ); //current forward friction curve

        private TireFrictionCurve _sideFrictionCurve = new TireFrictionCurve(
                    0.03f, 1.0f, 0.04f, 1.05f, 0.7f
            ); //current sideways friction curve

        //set from get/set method
        private Vector3 _gravity = new Vector3(0, -9.81f, 0);

        //calced when the gravity vector is set
        private Vector3 _gNorm = new Vector3(0, -1, 0);

        //simple blind callback for when the wheel changes from !grounded to grounded,
        //the input variable is the wheel-local impact velocity
        private Action<Vector3> _onImpactCallback; 
        
        //if automatic updates are enabled, this field may optionally be populated with a pre-update callback method;
        //will be called directly prior to the wheels internal update code being processed
        private Action<TireCollider> _preUpdateCallback; 
        
        //if automatic updates are enabled, this field may optionally be populated with a post-update callback method;
        //will be called directly after the wheels internal update code processing.
        private Action<TireCollider> _postUpdateCallback; 
        
        //private vars with external get methods (cannot be set, for data viewing/debug purposes only)
        
        //cached internal utility vars
        
        //cached inertia inverse used to eliminate division operations from per-tick update code
        private float _inertiaInverse; 

        //cached radius inverse used to eliminate division operations from per-tick update code
        private float _radiusInverse; 

        //cached mass inverse used to eliminate division operations from per-tick update code
        private float _massInverse; 
        
        //internal friction model values
        private float _prevFLong;
        private float _prevFLat;
        private float _prevFSpring;
        private float _prevSuspensionCompression;

        //linear velocity of spring in m/s, derived from prevCompression - currentCompression along suspension axis
        private float _vSpring;
        
        //wheel axis directions are calculated each frame during update processing
        private Vector3 _wF, _wR;        //contact-patch forward and right directions
        private Vector3 _wheelUp;       //wheel up (suspension) direction
        private Vector3 _wheelForward;  //wheel forward direction (actual wheel, not contact patch)
        private Vector3 _wheelRight;    //wheel right direction (actual wheel, not contact patch)
        private Vector3 _localVelocity; //the wheel local velocity at that contact patch
        private Vector3 _localForce;    //the wheel(contact-patch?) local forces; x=lat, y=spring, z=long
        private float   _vWheel;        //linear velocity of the wheel at contact patch
        private float   _vWheelDelta;   //linear velocity delta between wheel and surface
        private Vector3 _hitPoint;      //world-space position of contact patch

        #endregion ENDREGION - Private variables

        #region REGION - Public accessible API get/set methods

        //get-set equipped field defs

        /// <summary>
        ///     Get/Set the rigidbody that the TireCollider applies forces to.  MUST be set manually after TireCollider component
        ///     is added to a GameObject.
        /// </summary>
        public Rigidbody ConnectedRigidbody { get; set; }

        /// <summary>
        ///     Get/Set the current spring stiffness value.  This is the configurable value that influences the 'springForce' used
        ///     in suspension calculations
        /// </summary>
        public float Spring { get; set; } = 10f;

        public float SpringCurve { get; set; } = 0f;

        /// <summary>
        ///     Get/Set the current damper resistance value.  This is the configurable value that influences the 'dampForce' used
        ///     in suspension calculations
        /// </summary>
        public float Damper { get; set; } = 2f;

        /// <summary>
        ///     Get/Set the current length of the suspension.  This is a ray that extends from the bottom of the wheel as
        ///     positioned at the wheel collider
        /// </summary>
        public float Length { get; set; } = 1f;

        /// <summary>
        ///     Get/Set the current wheel mass.  This determines wheel acceleration from torque (not vehicle acceleration; that is
        ///     determined by down-force).  Lighter wheels will slip easier from brake and motor torque.
        /// </summary>
        public float Mass
        {
            get => _wheelMass;
            set
            {
                _wheelMass       = value;
                MomentOfInertia = _wheelMass * _wheelRadius * _wheelRadius * 0.5f;
                _inertiaInverse  = 1.0f      / MomentOfInertia;
                _massInverse     = 1.0f      / _wheelMass;
            }
        }

        /// <summary>
        ///     Get/Set the wheel radius.  This determines the simulated size of the wheel, and along with mass determines the
        ///     wheel moment-of-inertia which plays into wheel acceleration
        /// </summary>
        public float Radius
        {
            get => _wheelRadius;
            set
            {
                _wheelRadius     = value;
                MomentOfInertia = _wheelMass * _wheelRadius * _wheelRadius * 0.5f;
                _inertiaInverse  = 1.0f      / MomentOfInertia;
                _radiusInverse   = 1.0f      / _wheelRadius;
            }
        }

        public float Width { get; set; } = 1f;

        /// <summary>
        ///     Get/Set the offset from hit-point along suspension vector where forces are applied.  1 = at wheel collider
        ///     location, 0 = at hit location; inbetween values lerp between them.
        /// </summary>
        public float ForceApplicationOffset
        {
            get => _suspensionForceOffset;
            set => _suspensionForceOffset = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Get/Set the current forward friction curve.  This determines the maximum available traction force for a given slip
        ///     ratio.  See the TireFrictionCurve class for more info.
        /// </summary>
        public TireFrictionCurve ForwardFrictionCurve
        {
            get => _fwdFrictionCurve;
            set
            {
                if (value != null)
                    _fwdFrictionCurve = value;
            }
        }

        /// <summary>
        ///     Get/Set the current sideways friction curve.  This determines the maximum available traction force for a given slip
        ///     ratio.  See the TireFrictionCurve class for more info.
        /// </summary>
        public TireFrictionCurve SidewaysFrictionCurve
        {
            get => _sideFrictionCurve;
            set
            {
                if (value != null)
                    _sideFrictionCurve = value;
            }
        }

        /// <summary>
        ///     Get/set the current forward friction coefficient; this is a direct multiple to the maximum available traction/force
        ///     from forward friction
        ///     <para />
        ///     Higher values denote more friction, greater traction, and less slip
        /// </summary>
        public float ForwardFrictionCoefficient { get; set; } = 1f;

        /// <summary>
        ///     Get/set the current sideways friction coefficient; this is a direct multiple to the maximum available
        ///     traction/force from sideways friction
        ///     <para />
        ///     Higher values denote more friction, greater traction, and less slip
        /// </summary>
        public float SideFrictionCoefficient { get; set; } = 1f;

        /// <summary>
        ///     Get/set the current surface friction coefficient; this is a direct multiple to the maximum available
        ///     traction for both forwards and sideways friction calculations
        ///     <para />
        ///     Higher values denote more friction, greater traction, and less slip
        /// </summary>
        public float SurfaceFrictionCoefficient { get; set; } = 1f;

        /// <summary>
        ///     Rolling resistance coefficient.  Determines the drag/friction applied to the wheel based on tire deformation.
        ///     Applied as a flat term multiplied by wheel load; independent of wheel RPM or slip ratios.
        /// </summary>
        public float RollingResistance { get; set; } = 0.005f;

        /// <summary>
        ///     Rotational resistance factor -- drag and friction caused by bearings, axles, differentials, gearing, etc.  Scales
        ///     linearly with wheel RPM; at zero rpm the torque will be zero, at max rpm the torque will be angularVelocity *
        ///     rotationalResistance * deltaTime.
        /// </summary>
        public float RotationalResistance { get; set; }

        /// <summary>
        ///     Get/set the actual brake torque to be used for wheel velocity update/calculations.  Should always be a positive
        ///     value; sign of the value will be determined dynamically.
        ///     <para />
        ///     Any braking-response speed should be calculated in the external module before setting this value.
        /// </summary>
        public float BrakeTorque
        {
            get => _currentBrakeTorque;
            set => _currentBrakeTorque = Mathf.Abs(value);
        }

        /// <summary>
        ///     Get/set the current motor torque value to be applied to the wheels.  Can be negative for reversable motors /
        ///     reversed wheels.
        ///     <para />
        ///     Any throttle-response/etc should be calculated in the external module before setting this value.
        /// </summary>
        public float MotorTorque { get; set; } = 0f;

        /// <summary>
        ///     Get/set the current steering angle to be used by wheel friction code.
        ///     <para />
        ///     Any steering-response speed should be calculated in the external module before setting this value.
        /// </summary>
        public float SteeringAngle { get; set; } = 0f;

        /// <summary>
        ///     Get/Set the gravity vector that should be used during calculations.  MUST be updated every frame that gravity
        ///     differs from the previous frame or undesired and inconsistent functioning will result.
        /// </summary>
        public Vector3 GravityVector
        {
            get => _gravity;
            set
            {
                _gravity = value;
                _gNorm   = _gravity.normalized;
            }
        }

        /// <summary>
        ///     Get/Set the suspension sweep type -- Raycast, Spherecast, or Capsulecast (enum value)
        /// </summary>
        public WheelSweepType SweepType { get; set; } = WheelSweepType.Ray;

        /// <summary>
        ///     Get/Set the friction model to be used -- currently only Standard is supported.
        /// </summary>
        public WheelFrictionType FrictionModel { get; set; } = WheelFrictionType.Standard;

        /// <summary>
        ///     Get/Set if the TireCollider should use its own FixedUpdate function or rely on external calling of the update
        ///     method.
        /// </summary>
        public bool AutoUpdateEnabled { get; set; } = false;

        /// <summary>
        ///     Get/Set if wheel-collider should effect forces along suspension normal (true) or hit-normal (false, default).  Used
        ///     by repulsors to enable motive repulsion.
        /// </summary>
        public bool UseSuspensionNormal { get; set; } = false;

        /// <summary>
        ///     Seat the reference to the wheel-impact callback.  This method will be called when the wheel first contacts the
        ///     surface, passing in thko0e wheel-local impact velocity (impact force is unknown)
        /// </summary>
        /// <param name="callback"></param>
        public void SetImpactCallback(Action<Vector3> callback)
        {
            _onImpactCallback = callback;
        }

        /// <summary>
        ///     Set the pre-update callback method to be called when automatic updates are used
        /// </summary>
        /// <param name="callback"></param>
        public void SetPreUpdateCallback(Action<TireCollider> callback)
        {
            _preUpdateCallback = callback;
        }

        /// <summary>
        ///     Set the post-update callback method to be called when automatic updates are used
        /// </summary>
        /// <param name="callback"></param>
        public void SetPostUpdateCallback(Action<TireCollider> callback)
        {
            _postUpdateCallback = callback;
        }

        /// <summary>
        ///     Return true/false if tire was grounded on the last suspension check
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        ///     Wheel rotation in revloutions per minute, linked to angular velocity (changing one changes the other)
        /// </summary>
        public float Rpm
        {
            // wWheel / (pi*2) * 60f
            // all values converted to combined constants
            get => AngularVelocity * 9.549296585f;
            set => AngularVelocity = value * 0.104719755f;
        }

        /// <summary>
        ///     angular velocity in radians per second, linked to rpm (changing one changes the other)
        /// </summary>
        public float AngularVelocity { get; private set; }

        public float LinearVelocity => AngularVelocity * _wheelRadius;

        /// <summary>
        ///     compression distance of the suspension system; 0 = max droop, max = max suspension length
        /// </summary>
        public float CompressionDistance { get; private set; }

        /// <summary>
        ///     Get/Set the current raycast layer mask to be used by the wheel-collider ray/sphere-casting.
        ///     <para />
        ///     This determines which colliders will be checked against for suspension positioning/spring force calculation.
        /// </summary>
        public int RaycastMask { get; set; } = ~(1 << 26);

        /// <summary>
        ///     Return the per-render-frame rotation for the wheel mesh
        ///     <para />
        ///     this value can be used such as wheelMeshObject.transform.Rotate(Vector3.right, getWheelFrameRotation(), Space.Self)
        /// </summary>
        /// <returns></returns>
        public float PerFrameRotation =>

            // returns rpm * 0.16666_ * 360f * secondsPerFrame
            // degrees per frame = (rpm / 60) * 360 * secondsPerFrame
            Rpm * 6 * Time.deltaTime;

        /// <summary>
        ///     The external additional down force to use for friction calculations.  Should be set by the vehicle controller in
        ///     cases of bump-stop compression being reached, to emulate friction even when external colliders are providing the
        ///     support/downforce.
        /// </summary>
        public float ExternalSpringForce { get; set; } = 0f;

        /// <summary>
        ///     If true, will use the 'externalHitPoint' as the suspension-sweep point.  (external hit point must be updated
        ///     manually).
        ///     This setting overrides the internal suspension sweep.
        /// </summary>
        public bool UseExternalHit { get; set; } = false;

        /// <summary>
        ///     Get/Set the world-coordinate hit point of the wheel sweep.  This point -must- be along the suspension axis as if it
        ///     were derived from a raycast.  Only used if 'useExternalHit == true'.
        /// </summary>
        public Vector3 ExternalHitPoint { get; set; } = Vector3.zero;

        /// <summary>
        ///     Get/Set the hit-normal used by the external hit point calculations.  Only used if 'useExternalHit == true'
        /// </summary>
        public Vector3 ExternalHitNormal { get; set; } = Vector3.up;

        /// <summary>
        ///     Get the calculated moment-of-inertia for the wheel
        /// </summary>
        public float MomentOfInertia { get; private set; } = 1.0f * 0.5f * 0.5f * 0.5f;

        /// <summary>
        ///     Returns the last calculated value for spring force, in newtons; this is the force that is exerted on rigidoby along
        ///     suspension axis
        ///     <para />
        ///     This already has dampForce applied to it; for raw spring force = springForce-dampForce
        /// </summary>
        public float SpringForce => _localForce.y + ExternalSpringForce;

        /// <summary>
        ///     Returns the last calculated value for damper force, in newtons
        /// </summary>
        public float DampForce { get; private set; }

        /// <summary>
        ///     Returns the last calculated longitudinal (forwards) force exerted by the wheel on the rigidbody
        /// </summary>
        public float LongitudinalForce => _localForce.z;

        /// <summary>
        ///     Returns the last calculated lateral (sideways) force exerted by the wheel on the rigidbody
        /// </summary>
        public float LateralForce => _localForce.x;

        /// <summary>
        ///     Returns the last caclulated longitudinal slip ratio; this is basically (vWheelDelta-vLong)/vLong with some error
        ///     checking, clamped to a 0-1 value; does not infer slip direction, merely the ratio
        /// </summary>
        public float LongitudinalSlip { get; private set; }

        /// <summary>
        ///     Returns the last caclulated lateral slip ratio; this is basically vLat/vLong with some error checking, clamped to a
        ///     0-1 value; does not infer slip direction, merely the ratio
        /// </summary>
        public float LateralSlip { get; private set; }

        /// <summary>
        ///     Returns the last calculated wheel-local velocity (velocity of the wheel, in the wheels' frame of reference)
        /// </summary>
        public Vector3 WheelLocalVelocity => _localVelocity;

        /// <summary>
        ///     Returns the last raycast collider hit.
        /// </summary>
        public Collider ContactColliderHit { get; private set; }

        /// <summary>
        ///     Returns the surface normal of the raycast collider that was hit
        /// </summary>
        public Vector3 ContactNormal { get; private set; }

        /// <summary>
        ///     Returns the -ray- hit position of the current compression value.
        ///     Will return incorrect results if wheel is not grounded (returns uncompressed position), or if used with
        ///     sphere/capsule sweeps (returns the position as if it was a raycast used)
        /// </summary>
        public Vector3 WorldHitPos =>
            _wheel.transform.position - _wheel.transform.up * (Length - CompressionDistance + _wheelRadius);

        #endregion ENDREGION - Public accessible methods, API get/set methods

        #region REGION - Update methods -- internal, external

        public void ResetState()
        {
            AngularVelocity = 0;
            _localForce     = Vector3.zero;
        }
        
        public void FixedUpdate()
        {
            if (!AutoUpdateEnabled)
                return;

            _preUpdateCallback?.Invoke(this);
            UpdateWheel();
            _postUpdateCallback?.Invoke(this);
        }

        /// <summary>
        ///     UpdateWheel() should be called by the controlling component/container on every FixedUpdate that this wheel should
        ///     apply forces for.
        ///     <para />
        ///     Collider and physics integration can be disabled by simply no longer calling UpdateWheel
        /// </summary>
        public void UpdateWheel()
        {
            if (ConnectedRigidbody == null)

                //this.rigidBody = gameObject.GetComponentUpwards<Rigidbody>();
                return;

            if (_wheel == null)
                _wheel = gameObject;

            var t = _wheel.transform;
            _wheelUp                   = t.up;
            _wheelForward              = Quaternion.AngleAxis(SteeringAngle, _wheelUp) * t.forward;
            _wheelRight                = -Vector3.Cross(_wheelForward, _wheelUp);
            _prevSuspensionCompression = CompressionDistance;
            _prevFSpring               = _localForce.y;
            var prevGrounded = IsGrounded;

            if (CheckSuspensionContact()) //suspension compression is calculated in the suspension contact check
            {
                //surprisingly, this seems to work extremely well...
                //there will be the 'undefined' case where hitNormal==wheelForward (hitting a vertical wall)
                //but that collision would never be detected anyway, as well as the suspension force would be undefined/uncalculated
                _wR = Vector3.Cross(ContactNormal, _wheelForward);
                _wF = -Vector3.Cross(ContactNormal, _wR);

                _wF = _wheelForward - ContactNormal * Vector3.Dot(_wheelForward, ContactNormal);
                _wR = Vector3.Cross(ContactNormal, _wF);

                //wR = wheelRight - hitNormal * Vector3.Dot(wheelRight, hitNormal);

                //no idea if this is 'proper' for transforming velocity from world-space to wheel-space; but it seems to return the right results
                //the 'other' way to do it would be to construct a quaternion for the wheel-space rotation transform and multiple
                // vqLocal = qRotation * vqWorld * qRotationInverse;
                // where vqWorld is a quaternion with a vector component of the world velocity and w==0
                // the output being a quaternion with vector component of the local velocity and w==0
                var worldVelocityAtHit = ConnectedRigidbody.GetPointVelocity(_hitPoint);
                
                if (ContactColliderHit && ContactColliderHit.attachedRigidbody)
                    worldVelocityAtHit -= ContactColliderHit.attachedRigidbody.GetPointVelocity(_hitPoint);
                
                _localVelocity.z = Vector3.Dot(worldVelocityAtHit, _wF);
                _localVelocity.x = Vector3.Dot(worldVelocityAtHit, _wR);
                _localVelocity.y = Vector3.Dot(worldVelocityAtHit, ContactNormal);

                CalcSpring();
                IntegrateForces();
                
                if (!prevGrounded) 
                    
                    //if was not previously grounded, call-back with impact data;
                    //we really only know the impact velocity
                    _onImpactCallback?.Invoke(_localVelocity);
            }
            else
            {
                IntegrateUngroundedTorques();
                IsGrounded         = false;
                _vSpring           = _prevFSpring = DampForce = _prevSuspensionCompression = CompressionDistance = 0;
                _localForce        = Vector3.zero;
                ContactNormal      = Vector3.zero;
                _hitPoint          = Vector3.zero;
                ContactColliderHit = null;
                _localVelocity     = Vector3.zero;
                LateralSlip = 0;
                LongitudinalSlip = 0;
            }
            
            ConnectedRigidbody.AddTorque(-MotorTorque * t.right);
        }

        /// <summary>
        ///     Should be called whenever the wheel collider is disabled -- clears out internal state data from the previous wheel
        ///     hit
        /// </summary>
        public void ClearGroundedState()
        {
            IsGrounded         = false;
            _vSpring            = _prevFSpring = DampForce = _prevSuspensionCompression = CompressionDistance = 0;
            _localForce         = Vector3.zero;
            ContactNormal      = Vector3.up;
            _hitPoint           = Vector3.zero;
            _localVelocity      = Vector3.zero;
            ContactColliderHit = null;
        }

        #endregion ENDREGION - Update methods -- internal, external

        #region REGION - Private/internal update methods

        /// <summary>
        ///     Integrate the torques and forces for a grounded wheel, using the pre-calculated fSpring downforce value.
        /// </summary>
        private void IntegrateForces()
        {
            CalcFriction();

            //anti-jitter handling code; if lateral or long forces are oscillating, damp them on the rebound
            //could possibly even zero them out for the rebound, but this method allows for some force
            const float fMult = 0.1f;
            if (_prevFLong < 0 && _localForce.z > 0 || _prevFLong > 0 && _localForce.z < 0)
                _localForce.z *= fMult;

            if (_prevFLat < 0 && _localForce.x > 0 || _prevFLat > 0 && _localForce.x < 0)
                _localForce.x *= fMult;
            
            Vector3 calculatedForces;

            if (UseSuspensionNormal)
            {
                calculatedForces = _wheel.transform.up * _localForce.y;
            }
            else
            {
                calculatedForces =  ContactNormal * _localForce.y;
                calculatedForces += CalcAG(ContactNormal, _localForce.y);
            }

            calculatedForces += _localForce.z * _wF;
            calculatedForces += _localForce.x * _wR;
            var forcePoint = _hitPoint;

            if (_suspensionForceOffset > 0)
            {
                var offsetDist = Length - CompressionDistance + _wheelRadius;
                forcePoint = _hitPoint + _wheel.transform.up * (_suspensionForceOffset * offsetDist);
            }

            ConnectedRigidbody.AddForceAtPosition(calculatedForces, forcePoint, ForceMode.Force);
            if (ContactColliderHit
             && ContactColliderHit.attachedRigidbody
             && !ContactColliderHit.attachedRigidbody.isKinematic)
                
                ContactColliderHit.attachedRigidbody.AddForceAtPosition(-calculatedForces, forcePoint, ForceMode.Force);
            
            _prevFLong = _localForce.z;
            _prevFLat  = _localForce.x;
        }

        /// <summary>
        ///     Calculate an offset to the spring force that will negate the tendency to slide down hill caused by suspension
        ///     forces.
        ///     Seems to mostly work and brings drift down to sub-milimeter-per-second.
        ///     Should be combined with some sort of spring/joint/constraint to complete the sticky-friction implementation.
        /// </summary>
        /// <param name="hitNormal"></param>
        /// <param name="springForce"></param>
        /// <returns></returns>
        private Vector3 CalcAG(Vector3 hitNormal, float springForce)
        {
            var agFix = new Vector3(0, 0, 0);

            // this is the amount of suspension force that is misaligning the vehicle
            // need to push uphill by this amount to keep the rigidbody centered along suspension axis
            var gravNormDot = Vector3.Dot(hitNormal, _gNorm);

            //this force should be applied in the 'uphill' direction
            var agForce = gravNormDot * springForce;

            //calculate uphill direction from hitNorm and gNorm
            
            // cross of the two gives the left/right of the hill
            var hitGravCross = Vector3.Cross(hitNormal, _gNorm);

            // cross the left/right with the hitNorm to derive the up/down-hill direction
            var upDown = Vector3.Cross(hitGravCross, hitNormal);

            // and pray that all the rhs/lhs coordinates are correct...
            var slopeLatDot  = Vector3.Dot(upDown, _wR);
            var slopeLongDot = Vector3.Dot(upDown, _wF);
            
            agFix = _wR * (agForce * slopeLatDot *  Mathf.Clamp(SideFrictionCoefficient, 0, 1));
            var vel = Mathf.Abs(_localVelocity.z);

            var totalForceDownwards = springForce 
                                    / Vector3.Dot(_gNorm, -_wheel.transform.up) 
                                    * _gNorm;
            var longitudinalDownTorque = Mathf.Abs(Vector3.Dot(totalForceDownwards, _wF) 
                                                        * ForwardFrictionCoefficient) 
                                            * Radius;
            var totalTorqueAgainst = BrakeTorque - Mathf.Sign(Vector3.Dot(_wF, _gNorm)) * MotorTorque;

            if (totalTorqueAgainst > longitudinalDownTorque && Mathf.Abs(MotorTorque) < BrakeTorque && vel < 4)
            {
                var mult = 1f;

                if (vel > 2)
                {
                    //if between 2m/s and 4/ms, lerp output force between them
                    //zero ouput at or above 4m/s, max output at or below 2m/s, intermediate force output inbetween those values
                    vel  -= 2;       //clamp to range 0-2
                    vel  *= 0.5f;    //clamp to range 0-1
                    mult =  1 - vel; //invert to range 1-0; with 0 being for input velocity of 4
                }
                
                agFix += slopeLongDot * Mathf.Clamp(ForwardFrictionCoefficient, 0, 1) * mult * agForce * _wF;
            }

            if (totalTorqueAgainst < longitudinalDownTorque && BrakeTorque > Mathf.Abs(MotorTorque) && vel < 2)
            {
                AngularVelocity += (longitudinalDownTorque - totalTorqueAgainst)
                                 * Mathf.Sign(Vector3.Dot(_gNorm, _wF))
                                 * _radiusInverse
                                 * Time.fixedDeltaTime;
            }

            return agFix;
        }

        /// <summary>
        ///     Integrate drive and brake torques into wheel velocity for when -not- grounded.
        ///     This allows for wheels to change velocity from user input while the vehicle is not in contact with the surface.
        ///     Not-yet-implemented are torques on the rigidbody due to wheel accelerations.
        /// </summary>
        private void IntegrateUngroundedTorques()
        {
            //velocity change due to motor; if brakes are engaged they can cancel this out the same tick
            //acceleration is in radians/second; only operating on fixedDeltaTime seconds, so only update for that length of time
            AngularVelocity += MotorTorque * _inertiaInverse * Time.fixedDeltaTime;

            if (AngularVelocity != 0)
            {
                var rotationalDrag = RotationalResistance * AngularVelocity * _inertiaInverse * Time.fixedDeltaTime;
                rotationalDrag = Mathf.Min(Mathf.Abs(rotationalDrag), Mathf.Abs(AngularVelocity))
                               * Mathf.Sign(AngularVelocity);
                AngularVelocity -= rotationalDrag;
            }

            if (AngularVelocity != 0)
            {
                // maximum torque exerted by brakes onto wheel this frame
                var wBrake = _currentBrakeTorque * _inertiaInverse * Time.fixedDeltaTime;

                // clamp the max brake angular change to the current angular velocity
                wBrake = Mathf.Min(Mathf.Abs(AngularVelocity), wBrake) * Mathf.Sign(AngularVelocity);

                // and finally, integrate it into wheel angular velocity
                AngularVelocity -= wBrake;
            }
        }

        /// <summary>
        ///     Uses either ray- or sphere-cast to check for suspension contact with the ground, calculates current suspension
        ///     compression, and caches the world-velocity at the contact point
        /// </summary>
        /// <returns></returns>
        private bool CheckSuspensionContact()
        {
            if (UseExternalHit)
            {
                var dist = (ExternalHitPoint - _wheel.transform.position).magnitude;
                CompressionDistance = Length + _wheelRadius - dist;
                ContactNormal       = ExternalHitNormal;
                _hitPoint            = ExternalHitPoint;
                ContactColliderHit  = null;
                IsGrounded          = true;

                return true;
            }

            switch (SweepType)
            {
                case WheelSweepType.Ray:
                    return SuspensionSweepRaycast();

                case WheelSweepType.Sphere:
                    return SuspensionSweepSpherecast();

                case WheelSweepType.Capsule:
                    return SuspensionSweepCapsuleCast();

                case WheelSweepType.SphereSlice:
                    return SuspensionSweepSphereSliceCast();

                default:
                    return SuspensionSweepRaycast();
            }
        }

        /// <summary>
        ///     Check suspension contact using a ray-cast; return true/false for if contact was detected
        /// </summary>
        /// <returns></returns>
        private bool SuspensionSweepRaycast()
        {
            RaycastHit hit;

            if (Physics.Raycast(
                _wheel.transform.position, 
                -_wheel.transform.up, 
                out hit, 
                Length + _wheelRadius, 
                RaycastMask,
                QueryTriggerInteraction.Ignore
            ))
            {
                CompressionDistance = Length + _wheelRadius - hit.distance;
                ContactNormal       = hit.normal;
                ContactColliderHit  = hit.collider;
                _hitPoint           = hit.point;
                IsGrounded          = true;

                return true;
            }

            IsGrounded = false;

            return false;
        }

        /// <summary>
        ///     Check suspension contact using a sphere-cast; return true/false for if contact was detected.
        /// </summary>
        /// <returns></returns>
        private bool SuspensionSweepSpherecast()
        {
            RaycastHit hit;

            //need to start cast above max-compression point, to allow for catching the case of @ bump-stop
            var rayOffset = _wheelRadius;

            if (Physics.SphereCast(
                _wheel.transform.position + _wheel.transform.up * rayOffset, Radius, -_wheel.transform.up, out hit,
                Length                   + rayOffset, RaycastMask, QueryTriggerInteraction.Ignore
            ))
            {
                CompressionDistance = Length + rayOffset - hit.distance;
                ContactNormal       = hit.normal;
                ContactColliderHit  = hit.collider;
                _hitPoint            = hit.point;
                IsGrounded          = true;

                return true;
            }

            IsGrounded = false;

            return false;
        }

        //TODO config specified 'wheel width'
        //TODO config specified number of capsules
        /// <summary>
        ///     less efficient and less optimal solution for skinny wheels, but avoids the edge cases caused by sphere colliders
        ///     <para />
        ///     uses 2 capsule-casts in a V shape downward for the wheel instead of a sphere;
        ///     for some collisions the wheel may push into the surface slightly, up to about 1/3 radius.
        ///     Could be expanded to use more capsules at the cost of performance, but at increased collision fidelity, by
        ///     simulating more 'edges' of a n-gon circle.
        ///     Sadly, unity lacks a collider-sweep function, or this could be a bit more efficient.
        /// </summary>
        /// <returns></returns>
        private bool SuspensionSweepCapsuleCast()
        {
            //create two capsule casts in a v-shape
            //take whichever collides first
            var wheelWidth = 0.3f;
            var capRadius  = wheelWidth * 0.5f;

            RaycastHit hit;
            RaycastHit hit1;
            RaycastHit hit2;
            bool       hit1b;
            bool       hit2b;
            var        startPos  = _wheel.transform.position;
            var        rayOffset = _wheelRadius;
            var        rayLength = Length      + rayOffset;
            var        capLen    = _wheelRadius - capRadius;
            var worldOffset =
                _wheel.transform.up
              * rayOffset; //offset it above the wheel by a small amount, in case of hitting bump-stop
            var capEnd1   = _wheel.transform.position + _wheel.transform.forward * capLen;
            var capEnd2   = _wheel.transform.position - _wheel.transform.forward * capLen;
            var capBottom = _wheel.transform.position - _wheel.transform.up      * capLen;
            hit1b = Physics.CapsuleCast(
                capEnd1 + worldOffset, capBottom + worldOffset, capRadius, -_wheel.transform.up, out hit1, rayLength,
                RaycastMask, QueryTriggerInteraction.Ignore
            );
            hit2b = Physics.CapsuleCast(
                capEnd2 + worldOffset, capBottom + worldOffset, capRadius, -_wheel.transform.up, out hit2, rayLength,
                RaycastMask, QueryTriggerInteraction.Ignore
            );

            if (hit1b || hit2b)
            {
                if (hit1b && hit2b)
                    hit = hit1.distance < hit2.distance ? hit1 : hit2;
                else if (hit1b)
                    hit = hit1;
                else if (hit2b)
                    hit = hit2;
                else
                    hit = hit1;
                
                CompressionDistance = Length + rayOffset - hit.distance;
                ContactNormal       = hit.normal;
                ContactColliderHit  = hit.collider;
                _hitPoint           = hit.point;
                IsGrounded          = true;

                return true;
            }

            IsGrounded = false;

            return false;
        }
        
        private bool SuspensionSweepSphereSliceCast()
        {
            //need to start cast above max-compression point, to allow for catching the case of @ bump-stop
            var rayOffset = _wheelRadius;

            if (Physics.SphereCast(
                _wheel.transform.position + _wheel.transform.up * rayOffset, 
                Radius, 
                -_wheel.transform.up,
                out var hit, 
                Length + rayOffset, 
                RaycastMask, 
                QueryTriggerInteraction.Ignore
            )
            && PointInWidthOfWheel(hit.point))
            {
                CompressionDistance = Length + rayOffset - hit.distance;
                ContactNormal       = hit.normal;
                ContactColliderHit  = hit.collider;
                _hitPoint           = hit.point;
                IsGrounded          = true;

                return true;
            }

            IsGrounded = false;

            return false;
        }

        private bool PointInWidthOfWheel(Vector3 point, Space space = Space.World)
        {
            if (space == Space.World)
                point = _wheel.transform.InverseTransformPoint(point);

            return -Width / 2 <= point.x && point.x <= Width / 2;
        }

        #region REGION - Friction model shared functions

        private void CalcSpring()
        {
            //calculate damper force from the current compression velocity of the spring; damp force can be negative
            _vSpring   = (CompressionDistance - _prevSuspensionCompression) / Time.fixedDeltaTime; //per second velocity
            DampForce = Damper                                            * _vSpring;

            //calculate spring force basically from displacement * spring along with a secondary exponential term
            // k = xy + axy^2
            var fSpring = Spring      * CompressionDistance
                        + SpringCurve * Spring * CompressionDistance * CompressionDistance;

            //integrate damper value into spring force
            fSpring += DampForce;

            //if final spring value is negative, zero it out; negative springs are not possible without attachment to the ground; gravity is our negative spring :)
            if (fSpring < 0)
                fSpring = 0;
            _localForce.y = fSpring;
        }

        private void CalcFriction()
        {
            switch (FrictionModel)
            {
                case WheelFrictionType.Standard:
                    CalcFrictionStandard();

                    break;

                case WheelFrictionType.Pacejka:
                    CalcFrictionPacejka();

                    break;

                case WheelFrictionType.PhysX:
                    CalcFrictionPhysx();

                    break;

                default:
                    CalcFrictionStandard();

                    break;
            }
        }

        /// <summary>
        ///     Returns a slip ratio between 0 and 1, 0 being no slip, 1 being lots of slip
        /// </summary>
        /// <param name="vLong"></param>
        /// <param name="vWheel"></param>
        /// <returns></returns>
        private float CalcLongSlip(float vLong, float vWheel)
        {
            float sLong = 0;

            if (vLong == 0 && vWheel == 0)
                return 0f;

            var a = Mathf.Max(vLong, vWheel);
            var b = Mathf.Min(vLong, vWheel);
            sLong = (a - b) / Mathf.Abs(a);
            sLong = Mathf.Clamp(sLong, 0, 1);

            return sLong;
        }

        /// <summary>
        ///     Returns a slip ratio between 0 and 1, 0 being no slip, 1 being lots of slip
        /// </summary>
        /// <param name="vLong"></param>
        /// <param name="vLat"></param>
        /// <returns></returns>
        private float CalcLatSlip(float vLong, float vLat)
        {
            float sLat = 0;

            if (vLat == 0) //vLat = 0, so there can be no sideways slip
                return 0f;
            if (vLong == 0) //vLat!=0, but vLong==0, so all slip is sideways
                return 1f;

            sLat = Mathf.Abs(Mathf.Atan(vLat / vLong)); //radians
            sLat = sLat * Mathf.Rad2Deg;                //degrees
            sLat = sLat / 90f;                          //percentage (0 - 1)

            return sLat;
        }

        #endregion ENDREGION - Friction calculations methods based on alternate

        #region REGION - Standard Friction Model

        // based on : http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html

        public void CalcFrictionStandard()
        {
            //initial motor/brake torque integration, brakes integrated further after friction applied
            //motor torque applied directly
            AngularVelocity +=
                MotorTorque
              * _inertiaInverse
              * Time.fixedDeltaTime; //acceleration is in radians/second; only operating on 1 * fixedDeltaTime seconds, so only update for that length of time

            //rolling resistance integration
            if (AngularVelocity != 0)
            {
                var fRollResist = _localForce.y * RollingResistance; //rolling resistance force in newtons
                var tRollResist = fRollResist  * _wheelRadius;       //rolling resistance as a torque
                var wRollResist =
                    tRollResist * _inertiaInverse * Time.fixedDeltaTime; //rolling resistance angular velocity change
                wRollResist     =  Mathf.Min(wRollResist, Mathf.Abs(AngularVelocity)) * Mathf.Sign(AngularVelocity);
                AngularVelocity -= wRollResist;
            }

            //rotational resistance integration
            if (AngularVelocity != 0)

                //float fRotResist = currentAngularVelocity * rotationalResistanceCoefficient;
                //float tRotResist = fRotResist * radiusInverse;
                //float wRotResist = tRotResist * inertiaInverse * Time.fixedDeltaTime;
                //currentAngularVelocity -= wRotResist;
                AngularVelocity -= AngularVelocity
                                 * RotationalResistance
                                 * _radiusInverse
                                 * _inertiaInverse
                                 * Time.fixedDeltaTime;

            // maximum torque exerted by brakes onto wheel this frame as a change in angular velocity
            var wBrakeMax = _currentBrakeTorque * _inertiaInverse * Time.fixedDeltaTime;

            // clamp the max brake angular change to the current angular velocity
            var wBrake = Mathf.Min(Mathf.Abs(AngularVelocity), wBrakeMax);

            // sign it opposite of current wheel spin direction
            // and finally, integrate it into wheel angular velocity
            AngularVelocity += wBrake * -Mathf.Sign(AngularVelocity);

            // this is the remaining brake angular acceleration/torque that can be used to counteract wheel acceleration
            // caused by traction friction
            var wBrakeDelta = wBrakeMax - wBrake;

            _vWheel           = AngularVelocity * _wheelRadius;
            LongitudinalSlip = CalcLongSlip(_localVelocity.z, _vWheel);
            LateralSlip      = CalcLatSlip(_localVelocity.z, _localVelocity.x);
            _vWheelDelta      = _vWheel - _localVelocity.z;

            var downforce = _localForce.y + ExternalSpringForce;
            var fLongMax = _fwdFrictionCurve.Evaluate(LongitudinalSlip)
                         * downforce
                         * ForwardFrictionCoefficient
                         * SurfaceFrictionCoefficient;
            var fLatMax = _sideFrictionCurve.Evaluate(LateralSlip)
                        * downforce
                        * SideFrictionCoefficient
                        * SurfaceFrictionCoefficient;

            // TODO - this should actually be limited by the amount of force necessary to arrest the velocity of this wheel in this frame
            // so limit max should be (abs(vLat) * sprungMass) / Time.fixedDeltaTime  (in newtons)
            _localForce.x = fLatMax;

            // using current down-force as a 'sprung-mass' to attempt to limit overshoot when bringing the velocity to zero
            if (_localForce.x > Mathf.Abs(_localVelocity.x) * downforce)
                _localForce.x = Mathf.Abs(_localVelocity.x) * downforce;

            // if (fLat > sprungMass * Mathf.Abs(vLat) / Time.fixedDeltaTime) { fLat = sprungMass * Mathf.Abs(vLat) * Time.fixedDeltaTime; }
            _localForce.x *= -Mathf.Sign(_localVelocity.x); // sign it opposite to the current vLat

            //angular velocity delta between wheel and surface in radians per second; radius inverse used to avoid div operations
            var wDelta = _vWheelDelta * _radiusInverse;

            //amount of torque needed to bring wheel to surface speed over one second
            var tDelta = wDelta * MomentOfInertia;

            //newtons of force needed to bring wheel to surface speed over one second; radius inverse used to avoid div operations
            // float fDelta = tDelta * radiusInverse; // unused
            //absolute value of the torque needed to bring the wheel to road speed instantaneously/this frame
            var tTractMax = Mathf.Abs(tDelta) / Time.fixedDeltaTime;

            //newtons needed to bring wheel to ground velocity this frame; radius inverse used to avoid div operations
            var fTractMax = tTractMax * _radiusInverse;

            //final maximum force value is the smallest of the two force values;
            // if fTractMax is used the wheel will be brought to surface velocity,
            // otherwise fLongMax is used and the wheel is still slipping but maximum traction force will be exerted
            fTractMax = Mathf.Min(fTractMax, fLongMax);

            // convert the clamped traction value into a torque value and apply to the wheel
            var tractionTorque = fTractMax * _wheelRadius * -Mathf.Sign(_vWheelDelta);

            // and set the longitudinal force to the force calculated for the wheel/surface torque
            _localForce.z = fTractMax * Mathf.Sign(_vWheelDelta);

            //use wheel inertia to determine final wheel acceleration from torques; inertia inverse
            //used to avoid div operations; convert to delta-time, as accel is normally radians/s
            var angularAcceleration = tractionTorque * _inertiaInverse * Time.fixedDeltaTime;

            //apply acceleration to wheel angular velocity
            AngularVelocity += angularAcceleration;

            //second integration pass of brakes, to allow for locked-wheels after friction calculation
            if (Mathf.Abs(AngularVelocity) < wBrakeDelta)
            {
                AngularVelocity = 0;
                wBrakeDelta -= Mathf.Abs(AngularVelocity);
                var fMax = Mathf.Max(0, Mathf.Abs(fLongMax) - Mathf.Abs(_localForce.z)); //remaining 'max' traction left
                var fMax2 = Mathf.Max(0, downforce * Mathf.Abs(_localVelocity.z) - Mathf.Abs(_localForce.z));
                var fBrakeMax = Mathf.Min(fMax, fMax2);
                _localForce.z += fBrakeMax * -Mathf.Sign(_localVelocity.z);
            }
            else
            {
                AngularVelocity +=
                    -Mathf.Sign(AngularVelocity)
                  * wBrakeDelta; //traction from this will be applied next frame from wheel slip, but we're integrating here basically for rendering purposes
            }

            CombinatorialFriction(fLatMax, fLongMax, _localForce.x, _localForce.z, out _localForce.x, out _localForce.z);

            //TODO technically wheel angular velocity integration should not occur until after the force is capped here, otherwise things will get out of synch
        }

        /// <summary>
        ///     Simple and effective; limit their sum to the absolute maximum friction that the tire
        ///     can ever produce, as calculated by the (averaged=/) peak points of the friction curve.
        ///     This keeps the total friction output below the max of the tire while allowing the greatest range of optimal output
        ///     for both lat and long friction.
        ///     -Ideally- slip ratio would be brought into the calculation somewhere, but not sure how it should be used.
        /// </summary>
        private void CombinatorialFriction(
            float latMax, float longMax, float fLat, float fLong, out float combLat, out float combLong
        )
        {
            var max = (_fwdFrictionCurve.Max + _sideFrictionCurve.Max) * 0.5f * (_localForce.y + ExternalSpringForce);
            var len = Mathf.Sqrt(fLat * fLat + fLong * fLong);

            if (len > max)
            {
                fLong /= len;
                fLat  /= len;
                fLong *= max;
                fLat  *= max;
            }

            combLat  = fLat;
            combLong = fLong;
        }

        #endregion ENDREGION - Standard Friction Model

        #region REGION - Alternate Friction Model - Pacejka

        // based on http://www.racer.nl/reference/pacejka.htm
        // and also http://www.mathworks.com/help/physmod/sdl/ref/tireroadinteractionmagicformula.html?requestedDomain=es.mathworks.com
        // and http://www.edy.es/dev/docs/pacejka-94-parameters-explained-a-comprehensive-guide/
        // and http://www.edy.es/dev/2011/12/facts-and-myths-on-the-pacejka-curves/
        // and http://www-cdr.stanford.edu/dynamic/bywire/tires.pdf

        public void CalcFrictionPacejka()
        {
            CalcFrictionStandard();
        }

        #endregion ENDREGION - Alternate friction model

        #region REGION - Alternate Friction Model - PhysX

        // TODO
        // based on http://www.eggert.highpeakpress.com/ME485/Docs/CarSimEd.pdf
        public void CalcFrictionPhysx()
        {
            CalcFrictionStandard();
        }

        #endregion ENDREGION - Alternate Friction Model 2

        public void DrawDebug()
        {
            if (!IsGrounded)
                return;

            Vector3 rayStart, rayEnd;
            var     vOffset = ConnectedRigidbody.velocity * Time.fixedDeltaTime;

            //draw the force-vector line
            rayStart = _hitPoint;

            //because localForce isn't really a vector... its more 3 separate force-axis combinations...
            rayEnd =  ContactNormal * _localForce.y;
            rayEnd += _wR            * _localForce.x;
            rayEnd += _wF            * _localForce.z;
            rayEnd += rayStart;

            //rayEnd = rayStart + wheel.transform.TransformVector(localForce.normalized) * 2f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.magenta);

            rayStart += _wheel.transform.up * 0.1f;
            rayEnd   =  rayStart + _wF * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.blue);

            rayEnd = rayStart + _wR * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.red);
        }

        #endregion ENDREGION - Private/internal update methods
        
        #region REGION - Helper Methods

        #endregion ENDREGION - Helper Methods
    }

    public enum WheelSweepType
    {
        Ray,
        Sphere,
        Capsule,
        SphereSlice
    }

    public enum WheelFrictionType
    {
        Standard,
        Pacejka,
        PhysX
    }
}
