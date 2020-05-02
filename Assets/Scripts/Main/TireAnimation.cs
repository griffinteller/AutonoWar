using UnityEngine;

namespace Main
{
    public class TireAnimation : MonoBehaviour
    {
        private ActionHandler _actionHandler;

        public float lerpSpeed;
        public Transform robot;


        public Transform wheelColliderParent;
        //private Dictionary<string, WheelCollider> _wheelCollidersDictionary;

        public void Start()
        {
            _actionHandler = transform.parent.parent.parent.GetComponent<ActionHandler>();
        }

        // Update is called once per frame
        public void Update()
        {
            foreach (Transform tireMeshParent in transform)
            {
                
                var wheelCollider = _actionHandler.CachedFind(tireMeshParent.name.Substring(0,
                    tireMeshParent.name.Length - 3)).GetComponent<WheelCollider>();


                tireMeshParent.Rotate(tireMeshParent.right,
                    wheelCollider.rpm * _actionHandler.internalNegation / 60 * 360 * Time.deltaTime, Space.World);

                /*var lerper = tireMeshParent.GetComponent<Lerper>();

                if (lerper.lerping) return;

                //Vector3.cr*/
                wheelCollider.GetWorldPose(out var pos, out _);
                tireMeshParent.position = pos;
                
            }
        }
    }
}