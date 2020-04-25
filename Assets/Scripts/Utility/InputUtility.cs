using UnityEngine;

namespace Utility
{
    public static class InputUtility
    {
        
        public static Vector2 GetMouseDelta()
        {
            
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
        }
        
    }
}