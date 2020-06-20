using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreboardCell
    {
        public byte DecimalPlaces
        {
            get => byte.Parse(_formatter.Substring(1));
            set => _formatter = "N" + value;
        }
        
        public float FloatValue
        {
            get
            {
                if (!IsFloat)
                    throw new InvalidOperationException("Cell is not numeric");
                
                return _floatValue;
            }
            set
            {
                if (!IsFloat)
                    throw new InvalidOperationException("Cell is not numeric");
                
                _floatValue = value;
                _text.text = value.ToString(_formatter);
            }
        }
        
        public string StringValue
        {
            get
            {
                if (IsFloat)
                    throw new InvalidOperationException("Cell is numeric");
                
                return _stringValue;
            }
            set
            {
                if (IsFloat)
                    throw new InvalidOperationException("Cell is numeric");
                
                _stringValue = value;
                _text.text = value;
            }
        }

        public object Value
        {
            get
            {
                if (IsFloat)
                    return FloatValue;
                
                return StringValue;
            }
        }
        
        public ScoreboardColumn Column { get; }
        public CellLocation CellLoc { get; }
        public int ActorNumber { get; }

        public bool IsFloat;
        public readonly GameObject GameObject;
        
        private readonly Text _text;
        private readonly Image _image;
        private string _formatter;
        private float _floatValue;
        private string _stringValue;
        
        public ScoreboardCell(GameObject gameObject, int actorNumber, ScoreboardColumn column, byte decimalPlaces = 0)
        {
            GameObject = gameObject;
            _text = gameObject.GetComponentInChildren<Text>();
            _image = gameObject.GetComponentInChildren<Image>();
            
            if (!_text)
                throw new ArgumentException("GameObject does not have text component!");
            
            var layoutElement = gameObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = column.Layout.DefaultWidth;
            if (column.Layout.Expand)
                layoutElement.preferredWidth = 9999;
            _text.alignment = column.Layout.TextAnchor;

            var textTransform = _text.GetComponent<RectTransform>();
            var buffer = column.Layout.TextBuffer;
            textTransform.offsetMin += new Vector2(buffer, 0);
            textTransform.offsetMax -= new Vector2(buffer, 0);
            
            Column = column;
            IsFloat = column.IsFloat;
            DecimalPlaces = decimalPlaces;
            ActorNumber = actorNumber;
            CellLoc = new CellLocation(actorNumber, column.Name);

            if (IsFloat)
            {
                if (column.InitialValue is int i)
                    FloatValue = i;
                else
                    FloatValue = (float) column.InitialValue;
            }
            else
                StringValue = (string) column.InitialValue;
        }

        public void SetColor(Color color)
        {
            _image.color = color;
        }
    }

    public class CellLocation
    {
        public int ActorNumber;
        public string ColumnName;

        public CellLocation(int actorNumber, string columnName)
        {
            ActorNumber = actorNumber;
            ColumnName = columnName;
        }

        public CellLocation()
        {
        }
    }

    [Serializable]
    public class CellLayout
    {
        public readonly bool Expand;
        public readonly float DefaultWidth;
        public readonly TextAnchor TextAnchor;
        public readonly float TextBuffer;

        public CellLayout(bool expand = false, float defaultWidth = 40,
            TextAnchor textAnchor = TextAnchor.MiddleCenter, float textBuffer = 10)
        {
            Expand = expand;
            DefaultWidth = defaultWidth;
            TextAnchor = textAnchor;

            if (textAnchor == TextAnchor.MiddleCenter)
                TextBuffer = 0;
            else
                TextBuffer = textBuffer;
        }

        public static CellLayout Default()
        {
            return new CellLayout();
        }
    }
}