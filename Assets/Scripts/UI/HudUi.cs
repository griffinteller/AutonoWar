using System;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace UI
{
    public class HudUi : MonoBehaviour
    {

        public Transform windowParent;
        public List<KeyCode> hotKeys;
        public List<UnityEvent> events;

        private readonly Dictionary<string, GameObject> _windows = new Dictionary<string, GameObject>();

        public void Start()
        {
            foreach (Transform window in windowParent)
            {
                _windows.Add(window.name, window.gameObject);
                window.gameObject.SetActive(false);
            }
        }

        public static void SetWindowOpen(GameObject window, bool open)
        {
            window.SetActive(open);
        }

        public void SetWindowOpen(string windowName, bool open)
        {
            SetWindowOpen(_windows[windowName], open);
        }

        public void ToggleWindowOpen(string windowName)
        {
            print(windowName);
            var window = _windows[windowName];
            SetWindowOpen(window, !window.activeSelf);
        }

        public void Update()
        {
            KeyCheck();
        }

        private void KeyCheck()
        {
            for (var i = 0; i < hotKeys.Count; i++)
            {
                var keycode = hotKeys[i];
                if (Input.GetKeyDown(keycode))
                {
                    events[i].Invoke();
                }
            }
        }

        public static void ReturnToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}