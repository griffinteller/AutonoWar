using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class ScoreboardRow : MonoBehaviour
    {
        public GameObject cellPrefab;
        public int actorNumber;
        public bool locked;
        public Color defaultColor;
        private readonly List<ScoreboardCell> _cells = new List<ScoreboardCell>();
        private readonly Dictionary<string, ScoreboardCell> _cellsByName = new Dictionary<string, ScoreboardCell>();

        public void BuildFromColumns(IEnumerable<ScoreboardColumn> columns)
        {
            MetaUtility.DestroyAllChildren(gameObject);
            foreach (var column in columns)
                AddCell(column);
        }

        public void AddCell(ScoreboardColumn column, int index = -1)
        {
            if (index == -1)
                index = _cells.Count;
            
            var cellObject = Instantiate(cellPrefab, transform);
            cellObject.transform.SetSiblingIndex(index);

            var cellInstance = new ScoreboardCell(cellObject, actorNumber, column, column.DecimalPlaces);
            _cells.Add(cellInstance);
            _cellsByName.Add(column.Name, cellInstance);
        }

        public void RemoveCell(ScoreboardCell cell)
        {
            Destroy(cell.GameObject);
            _cells.Remove(cell);
            _cellsByName.Remove(cell.Column.Name);
        }

        public void RemoveCell(string name)
        {
            RemoveCell(GetCell(name));
        }

        public void RemoveCell(int index)
        {
            RemoveCell(GetCell(index));
        }

        public ScoreboardCell GetCell(int index)
        {
            return _cells[index];
        }

        public ScoreboardCell GetCell(string name)
        {
            return _cellsByName[name];
        }
        
        public string GetStringValue(string name)
        {
            var cell = GetCell(name);
            return cell.StringValue;
        }

        public float GetFloatValue(string name)
        {
            var cell = GetCell(name);
            return cell.FloatValue;
        }

        public string GetStringValue(int index)
        {
            var cell = _cells[index];
            return cell.StringValue;
        }

        public float GetFloatValue(int index)
        {
            var cell = _cells[index];
            return cell.FloatValue;
        }

        public void SetCellColor(ScoreboardCell cell, Color color)
        {
            cell.SetColor(color);
        }

        public void SetCellColor(string columnName, Color color)
        {
            SetCellColor(GetCell(columnName), color);
        }

        public void SetAllCellColors(Color color)
        {
            foreach (var cell in _cells)
                cell.SetColor(color);
        }

        public void ResetAllCellColors()
        {
            SetAllCellColors(defaultColor);
        }
    }
}