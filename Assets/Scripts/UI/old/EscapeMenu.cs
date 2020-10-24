using GameDirection;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class EscapeMenu : MonoBehaviour
    {
        public Button buttonPrefab;

        public void Start()
        {
            var buttonInfos = GameObject.FindWithTag("GameDirector")
                .GetComponent<GameDirector>().GetEscapeMenuButtonInfo();

            var totalHeight = 0f;
            const float width = 300f;
            foreach (var buttonInfo in buttonInfos)
            {
                var buttonObj = Instantiate(buttonPrefab.gameObject, transform);
                buttonObj.GetComponentInChildren<Text>().text = buttonInfo.name;
                buttonObj.GetComponent<Button>().onClick =
                    MetaUtility.UnityEventToButtonClickedEvent(buttonInfo.clickEvent);

                var rect = buttonObj.GetComponent<RectTransform>().rect;
                totalHeight += rect.height;
            }

            var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            totalHeight += verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                                                           + verticalLayoutGroup.spacing * (buttonInfos.Count - 1);
            var totalWidth = verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right + width;
            GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, totalHeight);
        }
    }
}