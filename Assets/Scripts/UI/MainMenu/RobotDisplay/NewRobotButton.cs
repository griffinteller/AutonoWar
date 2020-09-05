using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace UI.MainMenu.RobotDisplay
{
    public class NewRobotButton : MonoBehaviour
    {
        public void OnMouseUpAsButton()
        {
            var data = new GameObject("BuildData", typeof(CrossSceneDataContainer))
                .GetComponent<CrossSceneDataContainer>();
            data.data.Add("robotName", null);
            SceneManager.LoadScene("BuildScene");
        }
    }
}