﻿using GameDirection;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class SinglePlayerCreation : MonoBehaviour
    {
        public OptionsSlider mapSlider;
        public OptionsSliderNumeric numControllableBotsSlider;
        public SinglePlayerDirector singlePlayerDirectorPrefab;

        public void StartGame()
        {
            var directorObject = Instantiate(singlePlayerDirectorPrefab.gameObject);
            var directorComponent = directorObject.GetComponent<SinglePlayerDirector>();

            directorComponent.Init();
            directorComponent.CurrentMap = (MapEnum) mapSlider.enumWrapper.Index;
            directorComponent.numControllableBots = (int) numControllableBotsSlider.slider.value;

            SceneManager.LoadScene(MapEnumWrapper.MapSceneNames[directorComponent.CurrentMap]);
        }
    }
}