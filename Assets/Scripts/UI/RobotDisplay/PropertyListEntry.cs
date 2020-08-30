using TMPro;
using UnityEngine;

namespace UI.RobotDisplay
{
    public class PropertyListEntry : MonoBehaviour
    {
        public TMP_Text keyText;
        public TMP_Text valueText;

        public void AdjustSize(float verticalMargin, float sideMargin)
        {
            var keyTextTransform = keyText.rectTransform;
            var currentKeyTextSize = keyTextTransform.sizeDelta;
            
            var valueTextTransform = valueText.rectTransform;
            var currentValueTextSize = valueTextTransform.sizeDelta;
            
            var maxHeight = Mathf.Max(keyText.GetPreferredValues(keyText.text).y, 
                valueText.GetPreferredValues(valueText.text).y);
            maxHeight += verticalMargin;
            
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, maxHeight);
            
            currentKeyTextSize.y = maxHeight;
            keyTextTransform.sizeDelta = currentKeyTextSize;

            currentValueTextSize.y = maxHeight;
            valueTextTransform.sizeDelta = currentValueTextSize;
            
            keyText.margin = new Vector4(sideMargin, 0, 0, 0);
            valueText.margin = new Vector4(0, 0, sideMargin, 0);
        }
    }
}