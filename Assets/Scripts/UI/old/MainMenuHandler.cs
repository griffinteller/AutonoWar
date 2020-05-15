using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace UI.old
{
    public class MainMenuHandler : MonoBehaviour
    {

        public List<CanvasGroup> mainPanels;
        public CanvasGroup homePanel;
        public string singlePlayerScene;

        public void Start()
        {
            
            DisableAllMainPanels();
            DisplayHomePanel();
        
        }

        public void DisplayPanel(CanvasGroup panel)
        {
            
            DisableAllMainPanels();
            UiUtility.EnableCanvasGroup(panel);
            
        }

        private void DisableAllMainPanels()
        {

            UiUtility.DisableAllPanels(mainPanels);

        }

        public void StartSingleplayer()
        {

            SceneManager.LoadScene(singlePlayerScene);

        }

        public void DisplayHomePanel()
        {

            //mainCamera.GetComponent<CameraPan>().enabled = true;
            DisableAllMainPanels();
            UiUtility.EnableCanvasGroup(homePanel);

        }
    
    }
}
