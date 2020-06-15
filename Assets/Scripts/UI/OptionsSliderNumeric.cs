using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OptionsSliderNumeric : MonoBehaviour
    {
        [SerializeField] private int maxValue;

        [SerializeField] private int minValue;

        public Slider slider;
        [SerializeField] private Text text;
        [SerializeField] private bool wholeNumbers;

        public void Start()
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = wholeNumbers;
            slider.onValueChanged.AddListener(UpdateText);

            UpdateText(slider.value);
        }

        private void UpdateText(float value)
        {
            text.text = value.ToString();
        }
    }
}