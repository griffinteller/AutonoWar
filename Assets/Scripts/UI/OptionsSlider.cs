using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class OptionsSlider : MonoBehaviour
    {

        [SerializeField] private EnumWrapperEnum _enumWrapperEnum;
        [SerializeField] private int minIndex;
        [SerializeField] private int maxIndex;
        [SerializeField] private Slider slider;
        [SerializeField] private Text text;

        [HideInInspector] public EnumWrapper enumWrapper;

        public void Start()
        {

            enumWrapper = EnumWrapperSelector.FromEnum(_enumWrapperEnum);

            slider.minValue = minIndex;
            slider.maxValue = maxIndex;
            slider.onValueChanged.AddListener(UpdateText);
            
            UpdateText(slider.value);
            
        }

        private void UpdateText(float value)
        {
            enumWrapper.Index = (int) value;
            text.text = enumWrapper.ToString();
        }

    }
}