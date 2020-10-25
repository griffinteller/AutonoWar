using System.Collections.Generic;
using JetBrains.Annotations;

namespace UI
{
    public class TableTextGen
    {
        [NotNull] public string[] columns;
        [CanBeNull] public List<string[]> data;

        public TableTextGen([NotNull] string[] columns, 
            [CanBeNull] List<string[]> data = null)
        {
            this.columns = columns;
            this.data = data;
        }
        
        public 
    }
}