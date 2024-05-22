using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.TextileStats {
    public class StuffTableGeneratorClassic : StuffTableGenerator {
        private (StuffCategoryDef stuffCatDef, int databaseIndex)[] lookupCategories;
        private (KindAndDef statKindAndDef, int databaseIndex)[] lookupStats;
        
        private string[] cachedCategories;
        private ColumnLabelHeader[] cachedColumnLabelHeaders;
        private KindAndDef[] cachedColumns;

        public override string[] Categories {
            get {
                this.EnsureCachedData();
                return this.cachedCategories;
            }
        }

        private void EnsureCachedData() {
            if (this.cachedCategories != null) {
                return;
            }

            var lookupCategories = new (StuffCategoryDef stuffCatDef, int databaseIndex)[] {
                (StuffTableDatabase.StuffCategory_Material, StuffTableDatabase.CatIndexMaterial),
                (StuffTableDatabase.StuffCategory_Apparell, StuffTableDatabase.CatIndexApparell),
                (StuffTableDatabase.StuffCategory_Building, StuffTableDatabase.CatIndexBuilding),
                (StuffCategoryDefOf.Fabric, StuffTableDatabase.CategoryDefs.IndexOf(StuffCategoryDefOf.Fabric)),
                (StuffCategoryDefOf.Leathery, StuffTableDatabase.CategoryDefs.IndexOf(StuffCategoryDefOf.Leathery)),
                (StuffCategoryDefOf.Metallic, StuffTableDatabase.CategoryDefs.IndexOf(StuffCategoryDefOf.Metallic)),
                (StuffCategoryDefOf.Stony, StuffTableDatabase.CategoryDefs.IndexOf(StuffCategoryDefOf.Stony)),
                (StuffCategoryDefOf.Woody, StuffTableDatabase.CategoryDefs.IndexOf(StuffCategoryDefOf.Woody)),
            };

            var labelCategories = (
                from si in lookupCategories
                select $"{si.stuffCatDef.label} ({StuffTableDatabase.InCategory[si.databaseIndex].Count})"
            ).ToArray();

            var columnLabelHeaders = new List<ColumnLabelHeader>();
            var statColumns = new List<(KindAndDef statKindAndDef, int databaseIndex)>();

            void addGroup(string title, params KindAndDef[] kindAndDefs) {
                var countAdded = 0;
                foreach (var kindAndDef in kindAndDefs) {
                    var databaseIndex = StuffTableDatabase.StatDefs.IndexOf(kindAndDef);
                    if (databaseIndex >= 0) {
                        ++countAdded;
                        statColumns.Add((kindAndDef, databaseIndex));
                    }
                }
                if (countAdded > 0) {
                    columnLabelHeaders.Add(new ColumnLabelHeader(title, countAdded));
                }
            }

            addGroup(
                "",
                new KindAndDef(StatKind.Special, SpecialDef.Stat_Label),
                new KindAndDef(StatKind.Special, SpecialDef.Stat_Count),
                new KindAndDef(StatKind.Base, StatDefOf.MarketValue),
                new KindAndDef(StatKind.Base, StatDefOf.Mass),
                new KindAndDef(StatKind.Base, StatDefOf.Flammability),
                new KindAndDef(StatKind.Base, DefDatabase<StatDef>.GetNamed("Textile_Softness", errorOnFail: false))
            );
            addGroup(
                "Beautify:",
                new KindAndDef(StatKind.Offset, StatDefOf.Beauty),
                new KindAndDef(StatKind.Factor, StatDefOf.Beauty)
            );
            addGroup(
                "Insulation:",
                new KindAndDef(StatKind.Base, StatDefOf.StuffPower_Insulation_Cold),
                new KindAndDef(StatKind.Base, StatDefOf.StuffPower_Insulation_Heat)
            );
            addGroup(
                "Armor:",
                new KindAndDef(StatKind.Base, StatDefOf.StuffPower_Armor_Sharp),
                new KindAndDef(StatKind.Base, StatDefOf.StuffPower_Armor_Blunt),
                new KindAndDef(StatKind.Base, StatDefOf.StuffPower_Armor_Heat)
            );
            addGroup(
                "Damage:",
                new KindAndDef(StatKind.Base, StatDefOf.SharpDamageMultiplier),
                new KindAndDef(StatKind.Base, StatDefOf.BluntDamageMultiplier)
            );

            this.lookupCategories = lookupCategories;
            this.cachedCategories = labelCategories;
            this.cachedColumnLabelHeaders = columnLabelHeaders.ToArray();
            this.lookupStats = statColumns.ToArray();
            this.cachedColumns = this.lookupStats.Select(si => si.statKindAndDef).ToArray();
        }

        protected override RowsAndCols GenerateRowsAndCols() {
            this.EnsureCachedData();

            RowsAndCols baseRowsAndCols;
            var oldCategoryIndex = this.CategoryIndex;
            var oldSortBy = this.SortBy;
            try {
                this.CategoryIndex = this.lookupCategories[oldCategoryIndex].databaseIndex;
                
                var sortByDatabaseIndex = this.lookupStats[oldSortBy >= 0 ? oldSortBy : ~oldSortBy].databaseIndex;
                this.SortBy = oldSortBy >= 0 ? sortByDatabaseIndex : ~sortByDatabaseIndex;
                
                baseRowsAndCols = base.GenerateRowsAndCols();
            } finally {
                this.CategoryIndex = oldCategoryIndex;
                this.SortBy = oldSortBy;

            }

            RowData makeRow(RowData baseRow) {
                var cellValues = new Dictionary<KindAndDef, float>();
                foreach (var statKindAndDef in this.cachedColumns) {
                    if (baseRow.cellValues.TryGetValue(statKindAndDef, out var value)) {
                        cellValues.Add(statKindAndDef, value);
                    }
                }
                return new RowData(baseRow.def, cellValues, baseRow.colorTexture);
            }
            
            var data = new RowsAndCols();
            data.rows = baseRowsAndCols.rows.Select(makeRow).ToArray();
            data.columns = this.cachedColumns;
            data.columnLabelHeaders = this.cachedColumnLabelHeaders;
            return data;
        }
    }
}
