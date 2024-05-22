using System.Linq;

namespace Revolus.TextileStats {
    public class BuildingTableGenerator : CommonTableGenerator {
        private int? categoryIndex;
        private int? sortBy;
        private string[] categories;

        public override int CategoryIndex {
            get => this.categoryIndex ?? BuildingTableDatabase.CatIndexAll;
            set => this.categoryIndex = value;
        }

        public override int SortBy {
            get => this.sortBy ?? BuildingTableDatabase.StatIndexLabel;
            set => this.sortBy = value;
        }

        public override string[] Categories {
            get {
                if (this.categories is null) {
                    this.categories = (
                        from index in Enumerable.Range(0, BuildingTableDatabase.CategoryDefs.Count)
                        select $"{BuildingTableDatabase.CategoryDefs[index].label} ({BuildingTableDatabase.InCategory[index].Count})"
                    ).ToArray();
                }
                return this.categories;
            }
        }

        protected override RowsAndCols GenerateRowsAndCols() {
            var data = new RowsAndCols();
            data.rows = BuildingTableDatabase.InCategory[this.CategoryIndex].ToArray();
            data.columns = BuildingTableDatabase.StatDefs.ToArray();
            return data;
        }
    }
}
