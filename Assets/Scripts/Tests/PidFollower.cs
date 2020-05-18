using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class PidFollower : MonoBehaviour
    {

        private const float A = 0f;
        private const float B = 10f;
        private const float C = 0f;
        
        private const int DerivativeSmoothing = 10;
        
        private readonly List<Vector3> previousErrors = new List<Vector3>();
        private readonly List<float> previousTimestamps = new List<float>();

        private Vector3 _errorIntegral;

        public void Update()
        {

            var mousePosition = Input.mousePosition;
            var desiredSpritePos = Camera.main.ScreenToWorldPoint(mousePosition);
            desiredSpritePos.z = 0;
            
            var currentPosition = transform.position;
            
            var error = desiredSpritePos - currentPosition;

            var errorDerivative = new Vector3();

            if (previousErrors.Count == DerivativeSmoothing)
            {

                var totalTime = previousTimestamps[previousTimestamps.Count - 1]
                                - previousTimestamps[0];
                
                errorDerivative = (error - previousErrors[0]) / totalTime;
                previousTimestamps.RemoveAt(0);
                previousErrors.RemoveAt(0);

            }

            _errorIntegral += error * Time.deltaTime;

            print(errorDerivative);
            
            currentPosition.x += (A * error.x + B * errorDerivative.x + C * _errorIntegral.x) 
                                 * Time.deltaTime;
            currentPosition.y += (A * error.y + B * errorDerivative.y + C * _errorIntegral.y) 
                                 * Time.deltaTime;

            transform.position = currentPosition;
            
            previousErrors.Add(error);
            previousTimestamps.Add(Time.time);

        }
    }
}