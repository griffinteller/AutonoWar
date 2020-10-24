using UnityEngine;

namespace UI
{
    public class DisplayRotate : MonoBehaviour
    {
        [SerializeField] private float rotationsPerSecond;

        public void Update()
        {
            transform.Rotate(Vector3.up, rotationsPerSecond * 360f * Time.deltaTime, Space.World);
        }
    }
}