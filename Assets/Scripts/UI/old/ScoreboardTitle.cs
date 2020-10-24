using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreboardTitle : ScoreboardRow
    {
        public override void AddCell(ScoreboardColumn column, int index = -1)
        {
            base.AddCell(column, index);

            var cell = Cells[Cells.Count - 1];
            cell.IsFloat = false;
            cell.StringValue = column.Name;
        }
    }
}