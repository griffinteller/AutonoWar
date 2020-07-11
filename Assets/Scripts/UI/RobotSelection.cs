using Building;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace UI
{
    public class RobotSelection : MonoBehaviour
    {
        public void StartBuildSceneWithSettings()
        {
            string robotName = null;

            var buildDataObj = new GameObject(
                "BuildData",
                typeof(CrossSceneDataContainer));
            var buildData = buildDataObj.GetComponent<CrossSceneDataContainer>();
            buildData.data.Add("robotName", robotName);
            
            DontDestroyOnLoad(buildDataObj);

            SceneManager.LoadScene(MetaUtility.BuildSceneName);
        }
    }
}