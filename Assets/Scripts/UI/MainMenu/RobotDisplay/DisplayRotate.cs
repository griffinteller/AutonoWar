using UnityEngine;

namespace UI.MainMenu.RobotDisplay
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