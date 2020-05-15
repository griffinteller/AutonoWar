using ExitGames.Client.Photon;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ServerSettingsHandler : MonoBehaviour
    {
        [SerializeField] private OptionsSlider gameModeOptionsSlider;
        [SerializeField] private OptionsSlider mapOptionsSlider;
        [SerializeField] private OptionsSliderNumeric maxPlayersOptionsSlider;

        [SerializeField] private InputField nameInputField;

        public Hashtable GetServerDescription()
        {
            return new Hashtable
            {
                {"name", nameInputField.text},
                {"gameMode", (GameModeEnum) gameModeOptionsSlider.enumWrapper.Index},
                {"map", (MapEnum) mapOptionsSlider.enumWrapper.Index},
                {"maxPlayers", (byte) maxPlayersOptionsSlider.slider.value}
            };
        }
    }
}