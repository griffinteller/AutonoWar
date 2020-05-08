using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class UIUtility : MonoBehaviour
    {
        
        public static void EnableCanvasGroup(CanvasGroup panel)
        {

            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;

        }
    
        public static void DisableCanvasGroup(CanvasGroup panel)
        {

            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;

        }
        
        public static void DisableAllPanels(IEnumerable<CanvasGroup> panels)
        {
            
            foreach (var panel in panels)
            {
                
                DisableCanvasGroup(panel);
                
            }
            
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
        
    }
}