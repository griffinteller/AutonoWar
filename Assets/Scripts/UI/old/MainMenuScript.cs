using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.old
{
    public class MainMenuScript : MonoBehaviour
    {

        public GameObject mainMenu;
        public GameObject multiplayerMainMenu;
        public GameObject nameEnterMenu;
        public GameObject multiplayerRoom;

        public TMP_InputField nameEnterField;

        public Button continueButton;
        
        private GameObject _mainMenuInstance;
        private GameObject _multiplayerMainMenuInstance;
        private GameObject _nameEnterMenuInstance;
        private GameObject _multiplayerRoomInstance;
        
        private const string PlayerPrefsNameKey = "NickName";

        public void Start()
        {

            _mainMenuInstance = EnableMenu(mainMenu, transform);

        }

        public void OpenSinglePlayerScene()
        {
            
            SceneManager.LoadScene("SinglePlayerScene");
            
        }

        private void DisableMenu(GameObject menu)
        {

            Destroy(menu);

        }

        private GameObject EnableMenu(GameObject menu, Transform parent)
        {

            return Instantiate(menu, parent);

        }

        public void SwitchToNameEnter()
        {

            DisableMenu(_mainMenuInstance.gameObject);
            _multiplayerMainMenuInstance = EnableMenu(multiplayerMainMenu, transform);
            _nameEnterMenuInstance = EnableMenu(nameEnterMenu, _multiplayerMainMenuInstance.transform.Find("Panel"));
            nameEnterField = GameObject.Find("EnterNameField").GetComponent<TMP_InputField>();
            continueButton = GameObject.Find("ContinueButton").GetComponent<Button>();
            FillInNameIfKnown();

        }

        public void FillInNameIfKnown()
        {

            if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
            {

                nameEnterField.text = PlayerPrefs.GetString(PlayerPrefsNameKey);
                UpdateContinueInteraction();

            }
            
        }

        public void ReturnToMainMenu()
        {
            
            foreach (Transform child in transform)
            {
                
                Destroy(child.gameObject);
                
            }

            _mainMenuInstance = EnableMenu(mainMenu, transform);

        }

        public void UpdateContinueInteraction()
        {

            if (nameEnterField.text != null && !nameEnterField.text.Equals(""))
            {

                continueButton.interactable = true;

            }
            
        }

        public void ContinueFromNameMenu()
        {

            var nickname = nameEnterField.text;
            PhotonNetwork.NickName = nickname;
            PlayerPrefs.SetString(PlayerPrefsNameKey, nickname);
            DisableMenu(_nameEnterMenuInstance.gameObject);
            _multiplayerRoomInstance = EnableMenu(multiplayerRoom, _multiplayerMainMenuInstance.transform.Find("Panel"));

        }

    }
}
