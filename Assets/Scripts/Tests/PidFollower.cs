using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class PidFollower : MonoBehaviour
    {

        private const float A = 0f;
        private const float B = 1f;
        private const int DerivativeSmoothing = 10;
        private List<Vector3> previousErrors;
        
        private List<float> previousTimestamps;
        private float totalTime;
        
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

                errorDerivative = (error - previousErrors[0]) / totalTime;

            }

            print(errorDerivative);
            
            currentPosition.x += (A * error.x ) * Time.deltaTime;
            currentPosition.y += (A * error.y ) * Time.deltaTime;

            transform.position = currentPosition;

        }
    }
}