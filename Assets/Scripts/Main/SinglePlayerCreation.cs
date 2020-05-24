using GameDirection;
using Networking;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main
{
    public class SinglePlayerCreation : MonoBehaviour
    {
        public OptionsSliderNumeric numControllableBotsSlider;
        public OptionsSlider mapSlider;
        public SinglePlayerDirector singlePlayerDirectorPrefab;

        public const string HighlandsScene = "SinglePlayerHighlands";

        public void StartGame()
        {

            var directorObject = Instantiate(singlePlayerDirectorPrefab.gameObject);
            var directorComponent = directorObject.GetComponent<SinglePlayerDirector>();

            directorComponent.Init();
            directorComponent.map = (MapEnum) mapSlider.enumWrapper.Index;
            directorComponent.numControllableBots = (int) numControllableBotsSlider.slider.value;

            print(directorComponent.numControllableBots);
            SceneManager.LoadScene("SinglePlayerHighlands");

        }
    }
}