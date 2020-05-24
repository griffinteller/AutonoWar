using System;
using UnityEngine;

namespace Main
{
    public class Beacon : MonoBehaviour
    {
        public float multiplier = 1f;
        public LineRenderer lineRenderer;

        private float _startingWidth;

        public void Start()
        {
            _startingWidth = lineRenderer.widthMultiplier;
            //lineRenderer.colorGradient = GetGradient();
        }

        public void Update()
        {

            var camTransform = Camera.main.transform;
            var camDistance = (camTransform.position - transform.position).magnitude;
            lineRenderer.widthMultiplier = camDistance * multiplier * _startingWidth;
            lineRenderer.SetPosition(1, 
                new Vector3(0, 0, -lineRenderer.widthMultiplier / 2));
        }

        private Gradient GetGradient()
        {

            const float alpha = 1.0f;
            var gradient = new Gradient();
            gradient.SetKeys(
                new []
                {
                    new GradientColorKey(Color.cyan, 0.0f), 
                    new GradientColorKey(Color.cyan, 1.0f)
                },
                new [] 
                {
                    new GradientAlphaKey(0.0f, 0.0f), 
                    new GradientAlphaKey(alpha, 0.01f), 
                    new GradientAlphaKey(alpha, 1.0f) 
                }
            );

            return gradient;

        }

        public void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}