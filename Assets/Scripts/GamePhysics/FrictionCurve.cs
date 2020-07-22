using System;
using UnityEngine;

namespace GamePhysics
{
    [Serializable]
    public class FrictionCurve
    {
        public float extremumSlip = 0.2f;
        public float extremumCoefficient = 0.9f;
        public float asymtoteSlip = 0.5f;
        public float asymtoteCoefficient = 0.5f;

        public float GetCoefficientAt(float slipValue)
        {
            if (slipValue < 0)
                throw new ArgumentException("Slip value must be over one!");

            if (slipValue <= extremumSlip)
                return F1(slipValue);

            if (extremumSlip < slipValue && slipValue <= asymtoteSlip)
                return F2(slipValue);

            return asymtoteCoefficient;
        }

        private float F1(float slipValue)
        {
            return -(extremumCoefficient) / (extremumSlip * extremumSlip)
                   * (slipValue - extremumSlip) * (slipValue - extremumSlip)
                   + extremumCoefficient;
        }

        private float F2(float slipValue)
        {
            return (extremumCoefficient - asymtoteCoefficient) / 2
                   * (Mathf.Cos(Mathf.PI / (asymtoteSlip - extremumSlip) * (slipValue - extremumSlip)) - 1) 
                   + extremumCoefficient;
        }
    }
}