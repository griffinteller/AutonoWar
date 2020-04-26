using UI;
using UnityEngine;
using Utility;

namespace Main.Building
{
    public class BuildCameraMovement : MonoBehaviour
    {

        public float moveSensitivity;
        public float lookSensitivity;

        public BuildHandler buildHandler;

        private void Start()
        {
            moveSensitivity *= 0.5f;
            lookSensitivity *= 10f;
            
        }

        // Update is called once per frame
        public void Update()
        {

            var deltaTime = Time.smoothDeltaTime;

            var t = transform;
            var mouseDelta = InputUtility.GetMouseDelta();

            if (Input.GetMouseButton(0))
            {
                
                transform.Rotate(t.right, -mouseDelta.y * lookSensitivity * deltaTime, Space.World);
                if (t.up.y < 0)
                {
                
                    transform.Rotate(t.right, mouseDelta.y * lookSensitivity * deltaTime, Space.World);
                
                }
            
                transform.Rotate(Vector3.up, mouseDelta.x * lookSensitivity * deltaTime, Space.World);
                
            }
            var rotatedForwardVector = Vector3.ProjectOnPlane(t.forward, Vector3.up).normalized;
            
            t.Translate(rotatedForwardVector * (Input.GetAxis("Vertical") * moveSensitivity * deltaTime), Space.World);
            t.Translate(Input.GetAxis("Horizontal") * moveSensitivity * deltaTime, 0, 0);
            t.Translate(0, Input.GetAxis("UpDown") * moveSensitivity * deltaTime, 0, Space.World);
        
        }
    }
}
