using UnityEngine;

namespace Main
{

    public class WheelColliderFlippingBehaviour : MonoBehaviour
    {
        
        private Transform _robotBody;
        private ActionHandler _actionHandler;
        
        public void Start()
        {

            _actionHandler = GetComponent<ActionHandler>();
            _robotBody = transform.Find("Body");

        }

        public void Update()
        {

            var shouldAdjust = false;
            if (transform.up.y < 0) // if robot rot is incorrect
            {

                shouldAdjust = true;
                transform.Rotate(transform.forward, 180, Space.World); // flip outside coordinates so colliders work
                _robotBody.Rotate(_robotBody.forward, 180, Space.World); // flip body back

            }

            if (_robotBody.transform.up.y < 0)
            {

                _actionHandler.internalNegation = -1;

            }
            else
            {

                _actionHandler.internalNegation = 1;

            }

            if (shouldAdjust)
            {
                
                _actionHandler.InvertTires();
                
            }

        }
    }
    
}
