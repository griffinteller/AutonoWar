using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreboardCell
    {
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
                else
                    return StringValue;
            }
        }
        
        public ScoreboardColumn Column { get; }
        public CellLocation CellLoc { get; }
        public int ActorNumber { get; }

        public readonly bool IsFloat;
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
            
            Column = column;
            IsFloat = column.IsFloat;
            DecimalPlaces = decimalPlaces;
            ActorNumber = actorNumber;
            CellLoc = new CellLocation(actorNumber, column.Name);
            
            if (IsFloat)
                FloatValue = (float) column.InitialValue;
            else
                StringValue = (string) column.InitialValue;
        }

        public void SetColor(Color color)
        {
            _image.color = color;
        }
    }
}