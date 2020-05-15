using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OptionsSliderNumeric : MonoBehaviour
    {
        
        [SerializeField] private int minValue;
        [SerializeField] private int maxValue;
        [SerializeField] private bool wholeNumbers;
        [SerializeField] private Text text;
        
        public Slider slider;

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