using UnityEngine;

namespace Utility
{
    public class Billboard : MonoBehaviour
    {
        public void Update()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}