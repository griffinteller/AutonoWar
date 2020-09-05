using UnityEngine;

namespace Cam
{
    public class ViewportFollower : MonoBehaviour
    {
        private Camera _camera;
        public RectTransform viewport;
        public RectTransform canvasTransform;

        private RectTransform _parent;
        
        public void Awake()
        {
            _camera = GetComponent<Camera>();
            _parent = viewport.parent.GetComponent<RectTransform>();
        }

        public void Update()
        {
            var localSize = viewport.rect.size;
            var worldBottomLeft = _parent.TransformPoint(
                Vector2.Scale(viewport.anchorMin, _parent.rect.size) + viewport.offsetMin);

            var canvasBottomLeft = canvasTransform.InverseTransformPoint(worldBottomLeft);
            var canvasRelativeSize = canvasTransform.InverseTransformVector(
                Vector3.Scale(localSize, viewport.lossyScale));

            var canvasSize = canvasTransform.rect.size;
            
            var screenBottomLeft = new Vector2(canvasBottomLeft.x / canvasSize.x, 
                canvasBottomLeft.y / canvasSize.y);
            var screenSize = new Vector2(canvasRelativeSize.x / canvasSize.x, 
                canvasRelativeSize.y / canvasSize.y);

            _camera.rect = new Rect(screenBottomLeft, screenSize);
        }
    }
}