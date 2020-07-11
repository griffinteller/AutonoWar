using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class CrossSceneDataContainer : MonoBehaviour
    {
        public Dictionary<string, object> data = new Dictionary<string, object>();

        private bool _newScene;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _1, LoadSceneMode _2)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _newScene = true;
        }

        public void Update()
        {
            if (_newScene)
                Destroy(gameObject);
        }
    }
}