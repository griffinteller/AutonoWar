using UnityEngine;

namespace UI.old
{
    public class MenuBridge : MonoBehaviour
    {

        private MainMenuScript _mainMenuScript;

        public void Start()
        {

            _mainMenuScript = GameObject.Find("Canvas").GetComponent<MainMenuScript>();

        }

        public void ContinueFromNameEnter() => _mainMenuScript.ContinueFromNameMenu();

        public void OpenSinglePlayerScene() => _mainMenuScript.OpenSinglePlayerScene();

        public void GoToNameMenu() => _mainMenuScript.SwitchToNameEnter();

    }
}
