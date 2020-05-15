using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI.old
{
    public class MultiPlayerMenuScript : MonoBehaviour
    {

        public List<CanvasGroup> panels;

        public Button continueButton;

        public InputField nameInputField;

        public const string NickNameKey = "NickName";

        private void DisableAllPanels()
        {
            
            UiUtility.DisableAllPanels(panels);
            
        }

        public void DisplayPanel(CanvasGroup panel)
        {
            
            DisableAllPanels();
            UiUtility.EnableCanvasGroup(panel);
            
        }

        public void Start()
        {

            if (PlayerPrefs.HasKey(NickNameKey))
            {

                nameInputField.text = PlayerPrefs.GetString(NickNameKey);

            }
            
        }

        public void SetName()
        {
            
            PlayerPrefs.SetString(NickNameKey, nameInputField.text);
            PhotonNetwork.NickName = nameInputField.text;

        }
        
    }
}