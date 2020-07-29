using System;
using UnityEngine;

namespace GamePhysics
{
    [Serializable]
    public class FrictionCurve
    {
        public float adherentSlip = 0.5f;
        public float adherentCoef = 0.95f;
        public float peakSlip = 1.5f;
        public float peakCoef = 1.1f;
        public float asymtoteCoef = 0.5f;
        public float asymtoteDecayRate = 2.5f;
        public float stiffness = 1f;

        public float GetCoefficientAt(float slipValue)
        {
            if (slipValue < 0)
                slipValue *= -1;

            if (slipValue <= adherentSlip)
                return F1(slipValue);

            if (adherentSlip < slipValue && slipValue <= peakSlip)
                return F2(slipValue);
            
            if (peakSlip < slipValue)
                return F3(slipValue);

            return asymtoteCoef * stiffness;
        }
        
        private float F1(float slipValue)
        {
            return adherentCoef * stiffness / adherentSlip * slipValue;
        }

        private float F2(float slipValue)
        {

            var ix = adherentSlip * peakCoef / adherentCoef;
            var a = adherentSlip - 2 * ix + peakSlip;
            var b = (adherentSlip - ix
                     + Mathf.Sqrt((adherentSlip - ix) * (adherentSlip - ix) - (adherentSlip - slipValue) * a))
                / a - 1;

            return ((adherentCoef - peakCoef) * b * b + peakCoef) * stiffness;
        }

        private float F3(float slipValue)
        {
            var exponentBase = asymtoteDecayRate * (slipValue - peakSlip);
            return ((peakCoef - asymtoteCoef) * Mathf.Pow(2, -(exponentBase * exponentBase)) + asymtoteCoef) * stiffness;
        }
    }
}