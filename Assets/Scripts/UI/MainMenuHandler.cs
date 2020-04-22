using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
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

        public static void EnableCanvasGroup(CanvasGroup panel)
        {

            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;

        }
    
        private static void DisableCanvasGroup(CanvasGroup panel)
        {

            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;

        }

        public void DisplayPanel(CanvasGroup panel)
        {
            
            DisableAllMainPanels();
            EnableCanvasGroup(panel);
            
        }

        private void DisableAllMainPanels()
        {

            foreach (var panel in mainPanels)
            {
                
                DisableCanvasGroup(panel);
                
            }

        }

        public static void DisableAllPanels(List<CanvasGroup> panels)
        {
            
            foreach (var panel in panels)
            {
                
                DisableCanvasGroup(panel);
                
            }
            
        }
        
        public void StartSingleplayer()
        {

            SceneManager.LoadScene(singlePlayerScene);

        }

        public void DisplayHomePanel()
        {

            //mainCamera.GetComponent<CameraPan>().enabled = true;
            DisableAllMainPanels();
            EnableCanvasGroup(homePanel);

        }
    
    }
}
