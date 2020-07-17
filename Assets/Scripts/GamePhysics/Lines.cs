using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace GamePhysics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LineSegment
    {
        public readonly float2 Point1;
        public readonly float2 Point2;
        public readonly float A;
        public readonly float B;
        
        public Line Line => new Line(A, B);

        private readonly bool _xInOrder;
        private readonly bool _yInOrder;

        public LineSegment(float2 point1, float2 point2)
        {
            Point1 = point1;
            Point2 = point2;

            _xInOrder = point1.x < point2.x;
            _yInOrder = point1.y < point2.y;

            var determinant = point1.x * point2.y - point1.y * point2.x;
            A = (point2.y - point1.y) / determinant;
            B = (point2.x - point1.x) / determinant;
        }

        public bool BoxContains(float2 point)
        {
            bool x, y;

            if (_xInOrder)
                x = Point1.x < point.x && point.x < Point2.x;
            else
                x = Point1.x > point.x && point.x > Point2.x;
            
            if (_yInOrder)
                y = Point1.y < point.y && point.y < Point2.y;
            else
                y = Point1.y > point.y && point.y > Point2.y;

            return x && y;
        }

        public bool IsPointAbove(float2 point)
        {
            return ((A * point.x + B * point.y) > 1) == (B > 0);
        }

        public float2 PointAtPerpendicular(float2 point)
        {
            return new float2(
                (A - A * B * point.y + B * B * point.x) / (A * A + B * B),
                (B - A * B * point.x + A * A * point.y) / (A * A + B * B));
        }

        public static bool Intersection(LineSegment seg1, LineSegment seg2, out float2 intersection)
        {
            var determinant = seg1.A * seg2.B - seg1.B * seg2.A;
            
            var x = (seg2.B - seg1.B) / determinant;
            var y = -(seg2.A - seg1.A) / determinant;
            
            intersection = new float2(x, y);
            
            return seg1.BoxContains(intersection) && seg2.BoxContains(intersection);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Line
    {
        public readonly float A;
        public readonly float B;

        public Line(float a, float b)
        {
            A = a;
            B = b;
        }

        public Line(LineSegment segment)
        {
            A = segment.A;
            B = segment.B;
        }
        
        public Line(float2 point1, float2 point2)
        {
            var determinant = point1.x * point2.y - point1.y * point2.x;
            A = (point2.y - point1.y) / determinant;
            B = (point2.x - point1.x) / determinant;
        }
        
        public static float2 Intersection(Line line1, Line line2)
        {
            var determinant = line1.A * line2.B - line1.B * line2.A;
            
            var x = (line2.B - line1.B) / determinant;
            var y = -(line2.A - line1.A) / determinant;
            
            return new float2(x, y);
        }

        public Line TranslateAlongNormal(float distance)
        {
            var normal = new float2(A, B) / (float) Math.Sqrt(A * A + B * B);
            if (normal.y < 0)
                normal *= -1;

            if (normal.x == 0f)
                normal.x += float.Epsilon;
            if (normal.y == 0f)
                normal.y += float.Epsilon;

            normal *= distance;
            return new Line(1f / (1f / A + normal.x), 1f / (1f / B + normal.y));
        }

        public float GetYAtX(float x)
        {
            return (1f - A * x) / B;
        }
    }
}