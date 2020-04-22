using UnityEngine;

namespace Main
{

    public class TireAnimation : MonoBehaviour
    {

        public Transform wheelColliderParent;
        public Transform robot;
        public float lerpSpeed;
        
        private ActionHandler _actionHandler;


        public void Start()
        {
            
            _actionHandler = transform.parent.parent.parent.GetComponent<ActionHandler>();

        }

        // Update is called once per frame
        public void Update()
        {
            
            foreach (Transform tireMeshParent in transform)
            {
                
                var wheelCollider = wheelColliderParent.Find(tireMeshParent.name.Substring(0, 
                    tireMeshParent.name.Length - 3)).GetComponent<WheelCollider>();
                tireMeshParent.Rotate(tireMeshParent.right, 
                    wheelCollider.rpm * _actionHandler.internalNegation / 60 * 360 * Time.deltaTime, Space.World);
                
                var lerper = tireMeshParent.GetComponent<Lerper>();
                
                if (lerper.lerping)
                {
                    return;
                }

                wheelCollider.GetWorldPose(out var pos, out _);
                if (robot.up.y > 0)
                {

                    
                    tireMeshParent.position = pos;

                }
                else
                {
                    
                    lerper.LerpLocal(transform.parent.InverseTransformPoint(pos), lerpSpeed);
                    
                }

            }

        }
    }

}