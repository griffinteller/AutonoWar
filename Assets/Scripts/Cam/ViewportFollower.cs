using UnityEngine;

namespace Cam
{
    public class ViewportFollower : MonoBehaviour
    {
        private Camera _camera;
        public RectTransform viewport;

        public void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void Update()
        {
            var size = Vector2.Scale(viewport.rect.size, viewport.lossyScale);
            var rect = new Rect(viewport.position.x, viewport.position.y, size.x, size.y);
            rect.x -= viewport.pivot.x * size.x;
            rect.y -= viewport.pivot.y * size.y;

            _camera.rect = new Rect(rect.x / Screen.width, rect.y / Screen.height,
                rect.width / Screen.width, rect.height / Screen.height);
        }
    }
}