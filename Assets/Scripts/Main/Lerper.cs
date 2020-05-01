using UnityEngine;

namespace Main
{
    public class Lerper : MonoBehaviour
    {

        public bool lerping;
        
        private float _speed;
        private float _startTime;

        private Vector3 _start;
        private Vector3 _end;

        public void LerpLocal(Vector3 endParentLocal, float speed)
        {

            _startTime = Time.time;
            _start = transform.localPosition;
            _end = endParentLocal;
            _speed = speed;
            lerping = true;

        }

        public void Update()
        {

            if (lerping)
            {

                var t = (Time.time - _startTime) / _speed;
                if (t > 1)
                {

                    lerping = false;
                    return;

                }
                transform.localPosition = Vector3.Lerp(_start, _end, t);

            }
            
        }
    }
}
