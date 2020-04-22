using UnityEngine;
using UnityEngine.UI;

namespace Main
{
    public class ScriptEditorButtonBehaviour : MonoBehaviour
    {

        public GameObject editorField;
        public GameObject canvas;

        private GameObject _currentEditorField;

        private bool _showingScript = false;

        public void ToggleScriptEditor()
        {

            _showingScript = !_showingScript;
            if (_showingScript)
            {

                GetComponentInChildren<Text>().text = "Hide Script";
                _currentEditorField = Instantiate(editorField, canvas.transform);

            }
            else
            {

                GetComponentInChildren<Text>().text = "Show Script";
                Destroy(_currentEditorField);

            }

        }

    }
}
