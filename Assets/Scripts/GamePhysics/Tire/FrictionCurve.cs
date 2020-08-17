using UnityEngine;

namespace GamePhysics.Tire
{
    public class FrictionCurve
    {
        private readonly int arraySize;

        private readonly WheelFrictionCurvePoint[] asymptotePoints;
        private readonly WheelFrictionCurvePoint[] extremePoints;

        private float asymptoteSlip;  //Asymptote point slip (default 4).
        private float asymptoteValue; //Force at the asymptote slip (default 5500).
        private float extremumSlip;   //Extremum point slip (default 3).
        private float extremumValue;  //Force at the extremum slip (default 6000).

        public FrictionCurve()
        {
            extremumSlip   = 3;
            extremumValue  = 6000;
            asymptoteSlip  = 4;
            asymptoteValue = 5500;
            Stiffness      = 4;

            arraySize       = 50;
            extremePoints   = new WheelFrictionCurvePoint[arraySize];
            asymptotePoints = new WheelFrictionCurvePoint[arraySize];

            UpdateArrays();
        }

        public float ExtremumSlip
        {
            get => extremumSlip;
            set
            {
                extremumSlip = value;
                UpdateArrays();
            }
        }

        public float ExtremumValue
        {
            get => extremumValue;
            set
            {
                extremumValue = value;
                UpdateArrays();
            }
        }

        public float AsymptoteSlip
        {
            get => asymptoteSlip;
            set
            {
                asymptoteSlip = value;
                UpdateArrays();
            }
        }

        public float AsymptoteValue
        {
            get => asymptoteValue;
            set
            {
                asymptoteValue = value;
                UpdateArrays();
            }
        }

        public float Stiffness { get; set; }

        private void UpdateArrays()
        {
            //Generate the arrays
            for (var i = 0; i < arraySize; ++i)
            {
                extremePoints[i].TValue = i / (float) arraySize;
                extremePoints[i].SlipForcePoint = Hermite(
                    i / (float) arraySize,
                    Vector2.zero,
                    new Vector2(extremumSlip, extremumValue),
                    Vector2.zero,
                    new Vector2(extremumSlip * 0.5f + 1, 0)
                );

                asymptotePoints[i].TValue = i / (float) arraySize;
                asymptotePoints[i].SlipForcePoint = Hermite(
                    i / (float) arraySize,
                    new Vector2(extremumSlip, extremumValue),
                    new Vector2(asymptoteSlip, asymptoteValue),
                    new Vector2((asymptoteSlip - extremumSlip) * 0.5f + 1, 0),
                    new Vector2((asymptoteSlip - extremumSlip) * 0.5f + 1, 0)
                );
            }
        }

        public float Evaluate(float slip)
        {
            //The slip value must be positive.
            slip = Mathf.Abs(slip);

            if (slip < extremumSlip)
                return Evaluate(slip, extremePoints) * Stiffness;
            if (slip < asymptoteSlip)
                return Evaluate(slip, asymptotePoints) * Stiffness;

            return asymptoteValue * Stiffness;
        }

        private float Evaluate(float slip, WheelFrictionCurvePoint[] curvePoints)
        {
            var top    = arraySize - 1;
            var bottom = 0;
            var index  = (int) ((top + bottom) * 0.5f);

            var result = curvePoints[index];

            //Binary search the curve to find the corresponding t value for the given slip
            while (top != bottom && top - bottom > 1)
            {
                if (result.SlipForcePoint.x <= slip)
                    bottom = index;
                else if (result.SlipForcePoint.x >= slip)
                    top = index;

                index  = (int) ((top + bottom) * 0.5f);
                result = curvePoints[index];
            }

            var slip1  = curvePoints[bottom].SlipForcePoint.x;
            var slip2  = curvePoints[top].SlipForcePoint.x;
            var force1 = curvePoints[bottom].SlipForcePoint.y;
            var force2 = curvePoints[top].SlipForcePoint.y;

            var slipFraction = (slip - slip1) / (slip2 - slip1);

            return force1 * (1 - slipFraction) + force2 * slipFraction;

            ;
        }

        private static Vector2 Hermite(float t, Vector2 p0, Vector2 p1, Vector2 m0, Vector2 m1)
        {
            var t2 = t  * t;
            var t3 = t2 * t;

            return
                (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * p1 + (t3 - t2) * m1;
        }

        private struct WheelFrictionCurvePoint
        {
            public float   TValue;
            public Vector2 SlipForcePoint;
        }
    }
}
