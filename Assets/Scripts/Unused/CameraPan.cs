using UnityEngine;

namespace Unused
{
    public class CameraPan : MonoBehaviour
    {

        public float amount;
        public Transform viewObject;
    
        // Update is called once per frame
        void Update()
        {
        
            transform.RotateAround(viewObject.position, Vector3.up, amount);
            transform.LookAt(viewObject.position);
        
        }
    }
}
