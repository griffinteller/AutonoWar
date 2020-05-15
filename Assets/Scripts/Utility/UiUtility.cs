using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility
{
    public class UiUtility : MonoBehaviour
    {
        public const string NickNameKey = "NickName";
        private readonly List<CanvasGroup> _modalWindows = new List<CanvasGroup>();

        private readonly List<CanvasGroup> _panels = new List<CanvasGroup>();
        [SerializeField] private RectTransform modalWindowParent;

        [SerializeField] private RectTransform panelParent;
        [SerializeField] private CanvasGroup startPanel;
        [SerializeField] private GameObject topBar;

        public void Start()
        {
            GetPanels();
            GetModalWindows();

            SwitchToPanel(startPanel);
            DisableAllPanels(_modalWindows);
        }

        private void GetPanels()
        {
            foreach (RectTransform panel in panelParent)
                _panels.Add(panel.GetComponent<CanvasGroup>());
        }

        private void GetModalWindows()
        {
            foreach (RectTransform window in modalWindowParent)
                _modalWindows.Add(window.GetComponent<CanvasGroup>());
        }

        public static void EnableCanvasGroup(CanvasGroup panel)
        {
            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }

        public static void DisableCanvasGroupStatic(CanvasGroup panel)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        public static void DisableAllPanels(IEnumerable<CanvasGroup> panels)
        {
            foreach (var panel in panels)
                DisableCanvasGroupStatic(panel);
        }

        public void StoreNickName(InputField inputField)
        {
            PlayerPrefs.SetString(NickNameKey, inputField.text);
        }

        public void DisableAllPanels()
        {
            DisableAllPanels(_panels);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void ToggleCanvasGroupStatic(CanvasGroup panel)
        {
            panel.alpha = 1 - panel.alpha;
            panel.interactable = !panel.interactable;
            panel.blocksRaycasts = !panel.blocksRaycasts;
        }

        public void ToggleCanvasGroup(CanvasGroup panel)
        {
            ToggleCanvasGroupStatic(panel);
        }

        public void SwitchToPanel(CanvasGroup panel)
        {
            DisableAllPanels();
            EnableCanvasGroup(panel);
        }

        public void ShowModalWindow(CanvasGroup window)
        {
            EnableCanvasGroup(window);
        }

        public void HideModalWindow(CanvasGroup window)
        {
            DisableCanvasGroupStatic(window);
        }

        public void GoHome()
        {
            SwitchToPanel(startPanel);
            DisableAllPanels(_modalWindows);
            topBar.SetActive(true);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}