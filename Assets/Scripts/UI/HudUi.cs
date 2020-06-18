using System;
using System.Collections.Generic;
using GameDirection;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace UI
{
    public class HudUi : MonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _windows = new Dictionary<string, GameObject>();
        public List<UnityEvent> events;
        public List<KeyCode> hotKeys;

        public Transform windowParent;
        public HudElement[] elementEnums;
        public GameObject[] elementObjects;

        private GameObject _hudObject;

        private static readonly HashSet<HudElement> CompleteElementSet = new HashSet<HudElement>
        {
            HudElement.Clock
        };

        public void Awake()
        {
            _hudObject = GameObject.FindWithTag("Hud");
        }

        public void Start()
        {
            BuildFromDescription(GameObject.FindWithTag("GameDirector").GetComponent<GameDirector>().HudElements);
            
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
                if (Input.GetKeyDown(keycode)) events[i].Invoke();
            }
        }

        public static void ReturnToMainMenu()
        {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();
            
            SceneManager.LoadScene(0);
        }

        public void BuildFromDescription(HashSet<HudElement> elements)
        {
            foreach (var element in CompleteElementSet)
                if (!elements.Contains(element))
                {
                    elementObjects[Array.IndexOf(elementEnums, element)].SetActive(false);
                }
        }
    }

    public enum HudElement
    {
        Clock
    }
}