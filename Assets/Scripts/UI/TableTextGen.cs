using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace UI
{
    public class TableTextGen
    {
        public ColoredString[] columns;
        public List<ColoredString[]> data;

        public int width;
        public int color;

        public TableTextGen(
            ColoredString[] columns, 
            int color = 0, 
            int width = -1,
            [CanBeNull] List<ColoredString[]> data = null) // width of -1 means min width
        {
            this.columns = columns;
            this.data = data ?? new List<ColoredString[]>();
            this.color = color;
            this.width = width;
        }

        public string Generate()
        {
            string minWidth = GenerateMinWidth();

            return minWidth; // TODO: support custom widths
        }

        private string GenerateMinWidth()
        {
            ColoredString separator = new ColoredString("|", color);
            
            StringBuilder titleLineBuilder = new StringBuilder(); // build title line first
            int[] maxColumnWidths = GetMaxComlumnWidths();
            
            for (int i = 0; i < columns.Length; i++)
            {
                ColoredString column = columns[i];
                
                int columnWidth = maxColumnWidths[i];
                int delta = columnWidth - column.RenderedLength;
                
                titleLineBuilder.Append(" " 
                                        + column 
                                        + new string(' ', delta + 1) 
                                        + (i + 1 == columns.Length ? "" : separator.RawString));
            }

            StringBuilder[] dataLineBuilders = new StringBuilder[data.Count];
            for (int i = 0; i < dataLineBuilders.Length; i++) // build each line
            {
                dataLineBuilders[i] = new StringBuilder();
                StringBuilder lineBuilder = dataLineBuilders[i];

                for (int col = 0; col < data[i].Length; col++)
                {
                    ColoredString datum = data[i][col];
                    int columnWidth = maxColumnWidths[col];
                    Debug.Log(columnWidth);
                    int delta = columnWidth - datum.RenderedLength;
                    Debug.Log(delta);

                    lineBuilder.Append(" " 
                                       + datum 
                                       + new string(' ', delta + 1) 
                                       + (col + 1 == columns.Length ? "" : separator.RawString));
                }

                lineBuilder.Remove(lineBuilder.Length - 1, 1);
            }

            int totalWidth = 0;
            foreach (int width in maxColumnWidths)
                totalWidth += width + 3;
            totalWidth -= 1; // because there is no "|" at the end

            StringBuilder finalBuilder = new StringBuilder();
            finalBuilder.AppendLine(titleLineBuilder.ToString());
            finalBuilder.AppendLine(new string('-', totalWidth));
            
            foreach (StringBuilder lineBuilder in dataLineBuilders)
                finalBuilder.AppendLine(lineBuilder.ToString());

            return finalBuilder.ToString();
        }

        private int[] GetMaxComlumnWidths()
        {
            int[] maxColumnWidths = new int[columns.Length];
            for (int col = 0; col < columns.Length; col++)
                if (columns[col].RenderedLength > maxColumnWidths[col])
                    maxColumnWidths[col] = columns[col].RenderedLength;
            
            for (int row = 0; row < data.Count; row++)
                for (int col = 0; col < columns.Length; col++)
                    if (data[row][col].RenderedLength > maxColumnWidths[col])
                        maxColumnWidths[col] = data[row][col].RenderedLength;

            return maxColumnWidths;
        }
    }
}