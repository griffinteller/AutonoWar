using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.GameSelection
{
    public class ImageOptionPanel : MonoBehaviour
    {
        public enum SelectionMode
        {
            Single,
            Max,
            Any
        }
        
        public SelectionMode selectionMode;
        
        [Tooltip("Only used if selection mode is \"Max\"")]
        public float maxSelected;

        public int rowsPerPage;
        public int colsPerPage;
        public int verticalSpacing;
        public int horizontalSpacing;
        
        public List<ImageOption> options;
        
        [NonSerialized] 
        public readonly HashSet<ImageOption> selectedOptions = new HashSet<ImageOption>();

        public void Start()
        {
            SortOptions();
            InstantiateOptions();
        }

        private void InstantiateOptions()
        {
            var noPerPage = rowsPerPage * colsPerPage;
            var pages = (options.Count - 1) / noPerPage + 1;

            for (var page = 0; page < pages; page++)
            {
                var pageObj = new GameObject("Page" + page, typeof(RectTransform));

                var pageRectTransform = pageObj.GetComponent<RectTransform>();
                pageRectTransform.SetParent(transform);
                pageRectTransform.localPosition = Vector3.zero;
                pageRectTransform.localScale = Vector3.one;
                pageRectTransform.anchorMax = new Vector2(1, 1);
                pageRectTransform.anchorMin = new Vector2(0, 0);
                pageRectTransform.offsetMax = Vector2.zero;
                pageRectTransform.offsetMin = Vector2.zero;

                var pageLayoutGroup = pageObj.AddComponent<VerticalLayoutGroup>();
                pageLayoutGroup.childControlHeight = true;
                pageLayoutGroup.childControlWidth = true;
                pageLayoutGroup.childForceExpandHeight = true;
                pageLayoutGroup.childForceExpandWidth = true;
                pageLayoutGroup.padding  = new RectOffset(0, 0, verticalSpacing, verticalSpacing);
                pageLayoutGroup.spacing = verticalSpacing;
                pageLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

                for (var row = 0; row < rowsPerPage; row++)
                {
                    var rowObj = new GameObject("Row" + row, 
                        typeof(RectTransform), 
                        typeof(HorizontalLayoutGroup));
                    rowObj.transform.SetParent(pageRectTransform);
                    rowObj.transform.localScale = Vector3.one;
                    rowObj.transform.localPosition = Vector3.zero;

                    var rowLayoutGroup = rowObj.GetComponent<HorizontalLayoutGroup>();
                    rowLayoutGroup.padding = new RectOffset(horizontalSpacing, horizontalSpacing, 0, 0);
                    rowLayoutGroup.spacing = horizontalSpacing;
                    rowLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

                    var rowTransform = rowObj.transform;
                    for (var col = 0;
                        col < colsPerPage
                        && page * rowsPerPage * colsPerPage
                        + row * colsPerPage
                        + col < options.Count;
                        col++)
                    {
                        var i = page * rowsPerPage * colsPerPage
                                + row * colsPerPage
                                + col;
                        var optionTransform = Instantiate(options[i], rowTransform).transform;
                        optionTransform.localScale = Vector3.one;
                        optionTransform.localPosition = Vector3.zero;
                    }
                }

                pageObj.SetActive(false);
            }
            
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void SortOptions()
        {
            options.Sort((option1, option2) => string.Compare(
                option1.Text, option2.Text, StringComparison.Ordinal));
        }

        public bool SetSelected(ImageOption option, bool select = true)
        {
            switch (selectionMode)
            {
                case SelectionMode.Single:
                    if (!select)
                    {
                        ClearSelectedOptions();
                        return false;
                    }

                    if (selectedOptions.Count == 1)
                        ClearSelectedOptions();
                    else if (selectedOptions.Count != 0)
                        throw new Exception("Selected options is not in a legal state. Mode is Single, " +
                                            "and length is " + selectedOptions.Count);

                    AddSelectedOption(option);
                    return true;

                case SelectionMode.Max:
                    if (!select)
                    {
                        RemoveSelectedOption(option);
                        return false;
                    }
                    
                    if (selectedOptions.Count < maxSelected)
                    {
                        AddSelectedOption(option);
                        return true;
                    }

                    if (selectedOptions.Count > maxSelected)
                        throw new Exception("Selected options is not in a legal state. Mode is Max, " +
                                            "and length is " + selectedOptions.Count);

                    option.Selected = false;
                    return false; // we are at the max, so we didn't add one
                
                case SelectionMode.Any:
                    if (select)
                        AddSelectedOption(option);
                    else
                        RemoveSelectedOption(option);

                    return select;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddSelectedOption(ImageOption option)
        {
            option.Selected = true;
            selectedOptions.Add(option);
        }

        private void RemoveSelectedOption(ImageOption option)
        {
            option.Selected = false;
            selectedOptions.Remove(option);
        }

        private void ClearSelectedOptions()
        {
            foreach (var option in selectedOptions)
                option.Selected = false;
            
            selectedOptions.Clear();
        }
    }
}