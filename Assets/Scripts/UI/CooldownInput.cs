using Main;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CooldownInput : MonoBehaviour
    {
        public InGameUIScript inGameUiScript;
        public InputField inputField;

        private PlayerConnection _playerConnection;
        private ClassicTagScript _classicTagScript;

        public void Start()
        {

            _playerConnection = inGameUiScript.playerConnection;
        

        }

        public void SetCooldown()
        {

            if (!_classicTagScript)
            {

                _classicTagScript = _playerConnection.playerObject.GetComponent<ClassicTagScript>();

            }

            _classicTagScript.tagCooldown = float.Parse(inputField.text);
            inputField.Select();
            inputField.text = "";
            inputField.placeholder.GetComponent<Text>().text = "Current cooldown is " + _classicTagScript.tagCooldown;

        }
    
    }
}
