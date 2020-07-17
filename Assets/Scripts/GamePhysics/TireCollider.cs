using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace GamePhysics
{
    public class TireCollider : MonoBehaviour
    {
        [Tooltip("Meters")]
        public float tireRadius = 1f;
        
        [Tooltip("Proportional decrease in angular speed per second")]
        [Range(0, 1)]
        public float bearingFriction = 0.05f;
        
        [Tooltip("Distance above ground that force is applied. Can help with stability.")]
        public float forceAppPointDistance = 0f;
        
        [Tooltip("Distance above ground at which the tire is considered grounded.")]
        public float isGroundedDistance = 0.05f;

        public int numberOfRaycasts = 10;

        [Space(10)] 
        
        [Header("Suspension Spring")]
        [Tooltip("Newtons/Meter")]
        public float primaryConstant = 35000f;
        public float secondaryConstant = 100000f;
        
        [Tooltip("Length of suspension spring. Meters.")]
        public float length = 0f;

        [Space(10)]
        
        [Tooltip("Newton-meters")]
        public float motorTorque = 0f;
        
        [Tooltip("Newton-meters")]
        public float brakeTorque = 0f;
        
        public Vector3 LocalCenter { get; private set; }
        public Vector3 WorldCenter
        {
            get => transform.TransformPoint(LocalCenter);
            set => LocalCenter = transform.InverseTransformPoint(value);
        }

        public bool IsGrounded { get; private set; }

        private Rigidbody _rigidbody;
        private Transform _transform;

        private float _distanceBetweenCasts;

        private float RayLength => tireRadius + Mathf.Abs(-LocalCenter.y) + isGroundedDistance;
        
        private RaycastHit?[] _raycastHits;

        public void Start()
        {
            numberOfRaycasts = (int) (numberOfRaycasts / 2 * 2 + 1.5f); // make sure it's odd
            
            _rigidbody = GetComponentInParent<Rigidbody>();
            _raycastHits = new RaycastHit?[numberOfRaycasts];
            _pointsPlaceholder = new float2[numberOfRaycasts];
            _lineSegmentPlaceholder = new LineSegment[numberOfRaycasts];
            _linePlaceholder = new Line[numberOfRaycasts];

            _distanceBetweenCasts = 2 * tireRadius / (numberOfRaycasts - 1);

            if (!_rigidbody)
                throw new Exception("Tire Collider must belong to Rigidbody!");
        }

        public void FixedUpdate()
        {
            UpdateReferenceCache();
            PopulateHitsArray();
            AdjustTireToSurroundings();
        }

        private LineSegment[] _lineSegmentPlaceholder;
        private void AdjustTireToSurroundings()
        {
            LoadBaseLineSegments();
            LoadOutwardTranslatedLines();
            var numberOfSegments = LoadFinalSegments();

            if (TireAboveFinalSegments(numberOfSegments))
                return;

            MoveCenterToClosestLegalPoint(numberOfSegments);
        }

        private void MoveCenterToClosestLegalPoint(int numberOfSegments)
        {
            var candidates = new float2[numberOfSegments];
            var center = new float2();
            var minIndex = 0;
            var minDistance = 2 * tireRadius;
            for (var i = 0; i < numberOfSegments; i++)
            {
                var segment = _lineSegmentPlaceholder[i];
                var pointAtPerpendicular = segment.PointAtPerpendicular(center);
                if (segment.BoxContains(pointAtPerpendicular))
                {
                    var distance = Distance(center, pointAtPerpendicular);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minIndex = i;
                        candidates[i] = pointAtPerpendicular;
                    }
                }
                else
                {
                    var distance1 = Distance(center, segment.Point1);
                    var distance2 = Distance(center, segment.Point2);
                    if (distance1 < distance2 && distance1 < minDistance)
                    {
                        minDistance = distance1;
                        candidates[i] = segment.Point1;
                        minIndex = i;
                    }
                    else if (distance1 <= distance2 && distance1 < minDistance)
                    {
                        minDistance = distance2;
                        candidates[i] = segment.Point2;
                        minIndex = i;
                    }
                }
            }

            var newRelativeCenter = candidates[minIndex];
            LocalCenter = new Vector3(newRelativeCenter.x, newRelativeCenter.y, 0);
        }

        private bool TireAboveFinalSegments(float numberOfSegments)
        {
            float2 center = new float2(0, 0);
            for (var i = 0; i < numberOfSegments; i++)
                if (_lineSegmentPlaceholder[i].BoxContains(center) 
                    && !_lineSegmentPlaceholder[i].IsPointAbove(center))
                    return false;

            return true;
        }

        private int LoadFinalSegments()
        {
            var i = 0;
            var lastIntersection = new float2(-tireRadius, _linePlaceholder[0].GetYAtX(-tireRadius));
            var numberOfSegments = 0;
            while (i < numberOfRaycasts - 2) // number of raycasts - 1 lines, -1 because we check ahead
            {
                var j = i + 1;
                var minIntersectionX = tireRadius + 1;
                var intersection = new float2();
                while (j < numberOfRaycasts - 1)
                {
                    intersection = Line.Intersection(_linePlaceholder[i], _linePlaceholder[j]);
                    
                    if (intersection.x < minIntersectionX && intersection.x > lastIntersection.x)
                        minIntersectionX = intersection.x;

                    j++;
                }

                numberOfSegments++;
                
                if (j == numberOfRaycasts - 1)
                {
                    intersection = new float2(tireRadius, _linePlaceholder[i].GetYAtX(tireRadius));
                    _lineSegmentPlaceholder[i] = new LineSegment(lastIntersection, intersection);
                    break;
                }

                _lineSegmentPlaceholder[i] = new LineSegment(lastIntersection, intersection);
                lastIntersection = intersection;
                i += j;
            }

            return numberOfSegments;
        }

        private Line[] _linePlaceholder;
        private void LoadOutwardTranslatedLines()
        {
            for (var i = 0; i < numberOfRaycasts - 1; i++)
            {
                _linePlaceholder[i] = _lineSegmentPlaceholder[i].Line.TranslateAlongNormal(tireRadius);
            }
        }

        private float2[] _pointsPlaceholder;
        private void LoadBaseLineSegments()
        {
            for (var i = 0; i < numberOfRaycasts; i++)
            {
                var hit = _raycastHits[i];
                if (hit.HasValue)
                {
                    var distance = hit.Value.distance;
                    _pointsPlaceholder[i] = new float2(-tireRadius + i * numberOfRaycasts, -distance);
                }
                else
                {
                    _pointsPlaceholder[i] = new float2(-tireRadius + i * numberOfRaycasts, RayLength + 0.5f); 
                    // below the tire enough to not matter later
                }
            }
            
            for (var i = 0; i < numberOfRaycasts - 1; i++)
                _lineSegmentPlaceholder[i] = new LineSegment(_pointsPlaceholder[i], _pointsPlaceholder[i + 1]);
        }

        private void UpdateReferenceCache()
        {
            _transform = transform;
        }

        private void PopulateHitsArray()
        {
            for (var i = 0; i < numberOfRaycasts; i++)
            {
                var rayDirection = -_transform.up;
                if (Physics.Raycast(_transform.position + Vector3.right * (-tireRadius + i * numberOfRaycasts), 
                        rayDirection, out var hit, RayLength))
                    // ReSharper disable once PossibleNullReferenceException
                    _raycastHits[i] = hit;
                else
                    _raycastHits = null;
            }
        }

        private static float Distance(float2 a, float2 b)
        {
            return (float) Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }
    }
}