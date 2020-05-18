using System;
using UnityEngine;

namespace Main
{
    public class LineScreenScaler : MonoBehaviour
    {
        public float multiplier = 1f;
        public LineRenderer lineRenderer;

        private float _startingWidth;

        public void Start()
        {
            _startingWidth = lineRenderer.widthMultiplier;
        }

        public void Update()
        {

            transform.rotation = Quaternion.Euler(90, 0, 0);

            var camTransform = Camera.main.transform;

            var camDistanceX = transform.position.x - camTransform.position.x;
            var camDistanceZ = transform.position.z - camTransform.position.z;
            var camDistance = new Vector2(camDistanceX, camDistanceZ).magnitude;
            lineRenderer.widthMultiplier = camDistance * multiplier * _startingWidth;
        }
    }
}