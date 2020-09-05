using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UI.Scoreboard
{
    public class ScoreboardRow : MonoBehaviour
    {
        public GameObject cellPrefab;
        public int actorNumber;
        public bool locked;
        public Color defaultColor;
        protected readonly List<ScoreboardCell> Cells = new List<ScoreboardCell>();
        protected readonly Dictionary<string, ScoreboardCell> CellsByName = new Dictionary<string, ScoreboardCell>();

        public void BuildFromColumns(IEnumerable<ScoreboardColumn> columns)
        {
            MetaUtility.DestroyAllChildren(gameObject);
            foreach (var column in columns)
                AddCell(column);
        }

        public virtual void AddCell(ScoreboardColumn column, int index = -1)
        {
            if (index == -1)
                index = Cells.Count;
            
            var cellObject = Instantiate(cellPrefab, transform);
            cellObject.transform.SetSiblingIndex(index);

            var cellInstance = new ScoreboardCell(cellObject, actorNumber, column, column.DecimalPlaces);
            cellInstance.SetColor(defaultColor);
            Cells.Add(cellInstance);
            CellsByName.Add(column.Name, cellInstance);
        }

        public void RemoveCell(ScoreboardCell cell)
        {
            Destroy(cell.GameObject);
            Cells.Remove(cell);
            CellsByName.Remove(cell.Column.Name);
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
            return Cells[index];
        }

        public ScoreboardCell GetCell(string name)
        {
            return CellsByName[name];
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
            var cell = Cells[index];
            return cell.StringValue;
        }

        public float GetFloatValue(int index)
        {
            var cell = Cells[index];
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
            foreach (var cell in Cells)
                cell.SetColor(color);
        }

        public void ResetAllCellColors()
        {
            SetAllCellColors(defaultColor);
        }
    }
}