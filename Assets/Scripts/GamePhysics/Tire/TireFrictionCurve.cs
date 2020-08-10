using UnityEngine;

namespace GamePhysics.Tire
{
    public class TireFrictionCurve
    {
        private readonly AnimationCurve _curveData;
        private readonly Keyframe[]     _keyframes;
        private          float          _extSlip;
        private          float          _extVal;
        private          float          _asSlip;
        private          float          _asVal;
        private          float          _tailVal;
        
        /// <summary>
        /// No-param constructor that initializes with default curve values
        /// </summary>
        public TireFrictionCurve() : this(0.06f, 1.2f, 0.08f, 1.0f, 0.6f)
        {
            // NOOP
            // chained to the parameter constructor below with default values
        }

        /// <summary>
        /// Parameter constructor to initialize to a given curve setup
        /// </summary>
        /// <param name="extSlip"></param>
        /// <param name="extVal"></param>
        /// <param name="asSlip"></param>
        /// <param name="asVal"></param>
        /// <param name="tailVal"></param>
        public TireFrictionCurve(float extSlip, float extVal, float asSlip, float asVal, float tailVal)
        {
            _keyframes = new Keyframe[4];
            _curveData = new AnimationCurve();
            this._extSlip = extSlip;
            this._extVal = extVal;
            this._asSlip = asSlip;
            this._asVal = asVal;
            this._tailVal = tailVal;
            SetupCurve();
            //exportCurve("temp" + extSlip + ".png", 1024, 1024);
        }

        public float ExtremumSlip
        {
            get { return _extSlip; }
            set { _extSlip = value; SetupCurve(); }
        }

        public float ExtremumValue
        {
            get { return _extVal; }
            set { _extVal = value; SetupCurve(); }
        }

        public float AsymptoteSlip
        {
            get { return _asSlip; }
            set { _asSlip = value; SetupCurve(); }
        }

        public float AsymptoteValue
        {
            get { return _asVal; }
            set { _asVal = value;  SetupCurve(); }
        }

        public float TailValue
        {
            get { return _tailVal; }
            set { _tailVal = value;  SetupCurve(); }
        }

        public float Max
        {
            get { return Mathf.Max(_asVal, _extVal); }
        }

        /// <summary>
        /// Input = slip percent or cosin<para/>
        /// Value must be between 0...1 (inclusive)
        /// </summary>
        /// <param name="slipRatio"></param>
        /// <returns>Normalized force multiplier (normally 0...1, but can be more!)</returns>
        public float Evaluate(float slipRatio)
        {
            return _curveData.Evaluate(clampRatio(slipRatio));
        }

        /// <summary>
        /// Utility method to export this curve as a .png image file with the input width/height, stored to the input file name/location<para/>
        /// X range is clamped to 0-1 (slip ratio)
        /// Y range is clamped to 0-2 (output coefficient)
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ExportCurve(string fileName, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            int len = width;
            float input, output;

            float max = 2f;
            float outYF;
            int outY;
            //fill with black
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, Color.black);
                }
            }
            //plot the friction curve, extremum mapped to min = 0, max = 2
            for (int i = 0; i < len; i++)
            {
                input = (i + 1) / (float)width;//0-1 percentage value for time input
                output = _curveData.Evaluate(input);
                outYF = output / max;
                outY = (int)(outYF * height);
                texture.SetPixel(i, outY, Color.green);
            }
            byte[] fileBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fileName, fileBytes);
        }

        /// <summary>
        /// Setup the cached keyframes with the current wheel slip parameters
        /// </summary>
        private void SetupCurve()
        {
            //entry frame
            _keyframes[0].time = 0;
            _keyframes[0].value = 0;
            _keyframes[0].inTangent = 0f;
            _keyframes[0].outTangent = 0f;
            //extremum frame
            _keyframes[1].time = _extSlip;
            _keyframes[1].value = _extVal;
            _keyframes[1].inTangent = 0f;
            _keyframes[1].outTangent = 0f;
            //asymptote frame
            _keyframes[2].time = _asSlip;
            _keyframes[2].value = _asVal;
            _keyframes[2].inTangent = 0f;
            _keyframes[2].outTangent = 0f;
            //tail frame
            _keyframes[3].time = 1;
            _keyframes[3].value = _tailVal;
            _keyframes[3].inTangent = 0f;
            _keyframes[3].outTangent = 0f;

            //clear current data from the curve
            int len = _curveData.length;
            for (int i = len - 1; i >= 0; i--) { _curveData.RemoveKey(i); }

            //re-insert keyframes
            _curveData.AddKey(_keyframes[0]);
            _curveData.AddKey(_keyframes[1]);
            _curveData.AddKey(_keyframes[2]);
            _curveData.AddKey(_keyframes[3]);
        }

        /// <summary>
        /// Clamps an input slip ratio to the valid range of 0-1
        /// </summary>
        /// <param name="slipRatio"></param>
        /// <returns></returns>
        private float clampRatio(float slipRatio)
        {
            slipRatio = Mathf.Abs(slipRatio);
            slipRatio = Mathf.Min(1, slipRatio);
            slipRatio = Mathf.Max(0, slipRatio);
            return slipRatio;
        }

    }
}

