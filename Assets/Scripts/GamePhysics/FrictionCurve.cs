using System;
using System.Runtime.InteropServices;
using GameDirection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace GamePhysics
{
    public class FrictionCurve : ScriptableObject
    {
        [Serializable]
        public class Integration
        {
            public float[] relativeTorques;
            public float relativeTorqueStep;
            public float startingSlip;
            public float slipStep;
            public float[] slips;
            public FloatMatrix matrix;
            
            [NonSerialized] public FrictionCurve Curve;

            public Integration(FrictionCurve curve, float relativeTorqueRange,
                int relativeTorqueSamples = 50, float asymptoteDifferenceThreshold = 0.01f, int slipSamples = 100)
            {

                Curve = curve;
                relativeTorques = MathUtil.Linspace(-relativeTorqueRange, relativeTorqueRange,
                    relativeTorqueSamples);
                relativeTorqueStep = (relativeTorqueRange * 2)
                                     / (relativeTorqueSamples - 1);

                startingSlip = Mathf.Sqrt(-Mathf.Log((asymptoteDifferenceThreshold / (curve.y2 - curve.y3)))
                                               / Mathf.Log(2))
                    / curve.s + curve.x2;

                slipStep = startingSlip / slipSamples;
                slips = MathUtil.Linspace(slipStep, startingSlip, slipSamples);
                matrix = new FloatMatrix(relativeTorques.Length, slips.Length);

                GenerateMatrix();
            }

            private float GetSlipAfterTimeWithTorqueIndex(float adjustedDeltaT, float currentSlip, int torqueIndex)
            {
                if (currentSlip >= slips[slips.Length - 1]) // not strictly necessary, but a shortcut
                {
                    var asymptoteDeltaT = matrix[torqueIndex, slips.Length - 1];
                    if (asymptoteDeltaT < 0)
                        return adjustedDeltaT / -asymptoteDeltaT * slipStep + currentSlip;
                }
                
                int currentSlipIndex;
                var deltaTLeft = adjustedDeltaT - ApproxDeltaTToNearestTestedSlip(currentSlip, torqueIndex,
                    out currentSlipIndex);
                var negation = 1;
                var lastSlipIndex = currentSlipIndex;

                while (deltaTLeft > 0)
                {
                    if (currentSlipIndex == slips.Length)
                    {
                        var asymptoteDeltaT = matrix[torqueIndex, slips.Length - 1];
                        
                        if (asymptoteDeltaT < 0)
                            return deltaTLeft / -asymptoteDeltaT * slipStep + (currentSlipIndex + 1) * slipStep;

                        lastSlipIndex = currentSlipIndex;
                        currentSlipIndex = slips.Length - 1;
                        deltaTLeft -= asymptoteDeltaT;
                        continue;
                    }

                    if (currentSlipIndex < 0)
                    {
                        negation *= -1;
                        currentSlipIndex *= -1;
                        lastSlipIndex *= -1;
                        torqueIndex = relativeTorques.Length - torqueIndex - 1; // negate it
                        continue;
                    }

                    var deltaTThisStep = matrix[torqueIndex, currentSlipIndex];
                    lastSlipIndex = currentSlipIndex;
                    currentSlipIndex += Mathf.RoundToInt(-Mathf.Sign(deltaTThisStep));
                    deltaTLeft -= Mathf.Abs(deltaTThisStep);
                }
                
                // lastSlipIndex is not guaranteed to be in range
                var lastStepDeltaT = matrix[torqueIndex, lastSlipIndex];
                var proportionOverShot = deltaTLeft / lastStepDeltaT;
                var correction = -proportionOverShot * slipStep;

                return negation * (currentSlipIndex * slipStep + correction);
            }

            public float GetSlipAfterTime(float deltaT, float currentSlip, float motorTorque, float sprungForce,
                float radius, float momentOfInertia)
            {
                if (sprungForce == 0)
                    return currentSlip + motorTorque / momentOfInertia * radius * deltaT;
                
                deltaT *= sprungForce * radius * radius / momentOfInertia;
                var relativeTorque = motorTorque / sprungForce / radius;
                
                var torqueIndexLow = Mathf.Clamp((int) ((relativeTorque - relativeTorques[0]) / relativeTorqueStep),
                    0, relativeTorques.Length - 2);
                var torqueIndexHigh = torqueIndexLow + 1;
                var t = (relativeTorque - relativeTorques[torqueIndexLow])
                        / (relativeTorques[torqueIndexHigh] - relativeTorques[torqueIndexLow]);

                var finalSlipLowTorque = GetSlipAfterTimeWithTorqueIndex(deltaT, currentSlip, torqueIndexLow);
                var finalSlipHighTorque = GetSlipAfterTimeWithTorqueIndex(deltaT, currentSlip, torqueIndexHigh);

                return Mathf.LerpUnclamped(finalSlipLowTorque, finalSlipHighTorque, t);
            }

            private int2 GetNearestTorqueIndices(float torque)
            {
                var floorIndex = Mathf.Clamp((int) ((torque - relativeTorques[0]) / relativeTorqueStep), 
                    0, relativeTorques.Length - 1);
                var ceilIndex = floorIndex + 1;

                return new int2(floorIndex, ceilIndex);
            }

            public float ApproxDeltaTToNearestTestedSlip(float slip, int torqueIndex, out int finalSlipIndex)
            {
                // this isn't always strictly greater than slip, but the math works out
                var nearestSlipIndexAbove = Mathf.Clamp((int) (slip / slipStep), 0, slips.Length - 1);
                var totalDeltaT = matrix[torqueIndex, nearestSlipIndexAbove];
                var t = 1 - (slips[nearestSlipIndexAbove] - slip) / slipStep;

                finalSlipIndex = nearestSlipIndexAbove - 1;
                return t * totalDeltaT;
            }

            private void GenerateMatrix()
            {
                for (var i = 0; i < relativeTorques.Length; i++)
                    matrix[i, slips.Length - 1] = GetDeltaT(Curve.y3, relativeTorques[i]);

                for (var j = 0; j < slips.Length - 1; j++)
                {
                    var coef = Curve.GetCoefficientAt(slips[j]);
                    for (var i = 0; i < relativeTorques.Length; i++)
                        matrix[i, j] = GetDeltaT(coef, relativeTorques[i]);
                }
            }

            private float GetDeltaT(float coef, float relativeTorque)
            {
                return -slipStep / (-coef + relativeTorque);
            }
        }
        
        public float x1;
        public float y1;
        public float x2;
        public float y2;
        public float x3;
        public float y3;
        public float s;
        public float stiffness;
        [HideInInspector] public Integration integration;
        
        public int relativeTorqueSamples;
        public float asymptoteDifferenceThreshold;
        public int slipSamples;

        public void Setup()
        {
            s = Mathf.Sqrt(1 / (2 * Mathf.Log(2) * Mathf.Pow(x3 - x2, 2)));
            integration = new Integration(this, 2 * y2, 
                relativeTorqueSamples, asymptoteDifferenceThreshold, slipSamples);
        }

        public float GetCoefficientAt(float slipValue)
        {
            if (slipValue < 0)
                slipValue *= -1;

            if (slipValue <= x1)
                return F1(slipValue);

            if (x1 < slipValue && slipValue <= x2)
                return F2(slipValue);
            
            if (x2 < slipValue)
                return F3(slipValue);

            return y3 * stiffness;
        }
        
        private float F1(float slipValue)
        {
            return y1 * stiffness / x1 * slipValue;
        }

        private float F2(float slipValue)
        {

            var ix = x1 * y2 / y1;
            var a = x1 - 2 * ix + x2;
            var b = (x1 - ix
                     + Mathf.Sqrt((x1 - ix) * (x1 - ix) - (x1 - slipValue) * a))
                / a - 1;

            return ((y1 - y2) * b * b + y2) * stiffness;
        }

        private float F3(float slipValue)
        {
            var exponentBase = s * (slipValue - x2);
            return ((y2 - y3) * Mathf.Pow(2, -(exponentBase * exponentBase)) + y3) * stiffness;
        }
    }
}