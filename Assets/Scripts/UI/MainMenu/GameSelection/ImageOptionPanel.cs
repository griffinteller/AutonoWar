using System;
using System.Collections.Generic;
using UnityEngine;

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
        
        public List<ImageOption> options;
        
        [NonSerialized] 
        public readonly HashSet<ImageOption> selectedOptions = new HashSet<ImageOption>();

        public bool SetSelected(ImageOption option, bool select = true)
        {
            switch (selectionMode)
            {
                case SelectionMode.Single:
                    if (select)
                    {
                        if (selectedOptions.Count == 1)
                            selectedOptions.Clear();
                        else if (selectedOptions.Count != 0)
                            throw new Exception("Selected options is not in a legal state. Mode is Single, " +
                                                "and length is " + selectedOptions.Count);

                        selectedOptions.Add(option);
                        return true;
                    }
                    
                    selectedOptions.Clear();
                    return false;

                case SelectionMode.Max:
                    if (select)
                    {
                        if (selectedOptions.Count < maxSelected)
                        {
                            selectedOptions.Add(option);
                            return true;
                        }
                        
                        if (selectedOptions.Count > maxSelected)
                            throw new Exception("Selected options is not in a legal state. Mode is Max, " +
                                                "and length is " + selectedOptions.Count);
                        
                    }
                    else
                        selectedOptions.Remove(option);

                    break;
                
                case SelectionMode.Any:
                    if (select)
                        selectedOptions.Add(option);
                    else
                        selectedOptions.Remove(option);

                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}