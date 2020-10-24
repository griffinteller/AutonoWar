using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WebPageOpener : MonoBehaviour
    {
        public string url;

        public void Awake()
        {
            GetComponent<Button>().onClick.AddListener(delegate { OpenUrl(url); });
        }

        public static void OpenUrl(string url)
        {
            Application.OpenURL(url);
        }
    }
}