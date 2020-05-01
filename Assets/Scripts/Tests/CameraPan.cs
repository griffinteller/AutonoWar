using UnityEngine;

namespace Tests
{
    public class CameraPan : MonoBehaviour
    {

        public float amount;
        public Transform viewObject;
    
        // Update is called once per frame
        void Update()
        {
        
            transform.RotateAround(Vector3.zero, Vector3.up, amount);
            transform.LookAt(viewObject.position);
        
        }
    }
}
