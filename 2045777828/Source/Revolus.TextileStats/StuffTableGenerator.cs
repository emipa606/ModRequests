using System.Linq;

namespace Revolus.TextileStats {
    public class StuffTableGenerator : CommonTableGenerator {
        private int? categoryIndex;
        private int? sortBy;
        private string[] categories;

        public override int CategoryIndex {
            get => this.categoryIndex ?? StuffTableDatabase.CatIndexMaterial;
            set => this.categoryIndex = value;
        }

        public override int SortBy {
            get => this.sortBy ?? StuffTableDatabase.StatIndexLabel;
            set => this.sortBy = value;
        }

        public override string[] Categories {
            get {
                if (this.categories is null) {
                    this.categories = (
                        from index in Enumerable.Range(0, StuffTableDatabase.CategoryDefs.Count)
                        select $"{StuffTableDatabase.CategoryDefs[index].label} ({StuffTableDatabase.InCategory[index].Count})"
                    ).ToArray();
                }
                return this.categories;
            }
        }

        protected override RowsAndCols GenerateRowsAndCols() {
            var data = new RowsAndCols();
            data.rows = StuffTableDatabase.InCategory[this.CategoryIndex].ToArray();
            data.columns = StuffTableDatabase.StatDefs.ToArray();
            data.columnLabelHeaders = new ColumnLabelHeader[] {
                new ColumnLabelHeader("", StuffTableDatabase.CountStatSpecial),
                new ColumnLabelHeader("Offsets:", StuffTableDatabase.CountStatOffset),
                new ColumnLabelHeader("Factors:", StuffTableDatabase.CountStatFactor),
                new ColumnLabelHeader("Base stats:", StuffTableDatabase.CountStatBase),
            };
            return data;
        }
    }
}
