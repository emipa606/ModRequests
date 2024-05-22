using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Revolus.TextileStats {
    public abstract partial class CommonTableGenerator : ITableGenerator {
        protected class RowsAndCols {
            public RowData[] rows;
            public KindAndDef[] columns;
            public ColumnLabelHeader[] columnLabelHeaders;
        }

        protected abstract RowsAndCols GenerateRowsAndCols();

        public string[,] CellStrings {
            get;
            protected set;
        }

        public string[,] CellTooltips {
            get;
            protected set;
        }

        public Vector2[,] CellSizes {
            get;
            protected set;
        }

        public ColumnLabelHeader[] ColumnLabelHeaders {
            get;
            protected set;
        }

        public string[] ColumnLabels {
            get;
            protected set;
        }

        public int[] MaxColumnWidths {
            get;
            protected set;
        }

        public RowData[] RowData {
            get;
            protected set;
        }

        public abstract int CategoryIndex {
            get;
            set;
        }

        public abstract int SortBy {
            get;
            set;
        }

        public abstract string[] Categories {
            get;
        }

        protected static RowData[] SortRows(RowsAndCols data, int sortBy) {
            foreach (var stuff in data.rows) {
                stuff.UpdateCount();
            }

            var reversed = sortBy < 0;
            var sortIndex = reversed ? ~sortBy : sortBy;
            
            var stuffIndices = Enumerable.Range(0, data.rows.Length).ToList();

            // first sort by label
            var stricmp = StringComparer.InvariantCultureIgnoreCase;
            stuffIndices.Sort((int l, int r) => stricmp.Compare(data.rows[l].def.label, data.rows[r].def.label));
            
            // then sort by column (unless the selected column is the label)
            if (sortIndex != 0) {
                var key = data.columns[sortIndex];
                int sortFn(int l, int r) {
                    var hasL = data.rows[l].cellValues.TryGetValue(key, out var valueL);
                    var hasR = data.rows[r].cellValues.TryGetValue(key, out var valueR);
                    if (hasL && hasR) {
                        var result = valueL < valueR ? -1 : valueL > valueR ? +1 : 0;
                        return reversed ? -result : result;
                    } else {
                        return hasL ? -1 : hasR ? +1 : 0;
                    }
                }
                stuffIndices.SortStable(sortFn);
            } else if (reversed) {
                stuffIndices.Reverse();
            }

            return (from i in stuffIndices select data.rows[i]).ToArray();
        }

        public void ResetData() {
            var data = this.GenerateRowsAndCols();
            var countRows = data.rows.Length;
            var countCols = data.columns.Length;

            this.RowData = SortRows(data, this.SortBy);
            this.CellStrings = new string[countRows, countCols];
            this.CellTooltips = new string[countRows, countCols];
            this.CellSizes = new Vector2[countRows, countCols];
            this.MaxColumnWidths = new int[countCols];
            this.ColumnLabels = new string[countCols];
            this.ColumnLabelHeaders = data.columnLabelHeaders;

            for (var colIndex = 0; colIndex < countCols; ++colIndex) {
                var colStat = data.columns[colIndex];
                var columnLabel = colStat.def.label;
                switch (colStat.kind) {
                    case StatKind.Factor:
                        columnLabel += "\n(factor)";
                        break;
                    case StatKind.Offset:
                        columnLabel += "\n(offset)";
                        break;
                }
                this.ColumnLabels[colIndex] = columnLabel;
            }

            var oldFont = Text.Font;
            try {
                Text.Font = GameFont.Small;
                for (var rowIndex = 0; rowIndex < countRows; ++rowIndex) {
                    var rowStuff = this.RowData[rowIndex];
                    for (var colIndex = 0; colIndex < countCols; ++colIndex) {
                        var colStat = data.columns[colIndex];

                        float value = 0;
                        string s;
                        if (ReferenceEquals(colStat.def, SpecialDef.Stat_Label)) {
                            s = rowStuff.def.label;
                        } else if (rowStuff.cellValues.TryGetValue(colStat, out value)) {
                            s = FormatUtil.DoFormat(colStat, value);
                        } else {
                            continue;
                        }

                        var colSize = Text.CalcSize(s);

                        this.CellStrings[rowIndex, colIndex] = s;
                        this.CellSizes[rowIndex, colIndex] = colSize;

                        var oldMax = this.MaxColumnWidths[colIndex];
                        var newWidth = (int) Math.Round(colSize.x + 0.5);
                        if (newWidth > oldMax) {
                            this.MaxColumnWidths[colIndex] = newWidth;
                        }

                        string toolTip;
                        switch (colStat.kind) {
                            default:
                            case StatKind.Unset:
                                toolTip = "You can't see this message.";
                                break;
                            case StatKind.Special:
                                toolTip = (
                                    $"{rowStuff.def.label}\n{colStat.def.label}" +
                                    $"\n\nDebug:" +
                                    $"\n\u2014 {value:0.000000}" +
                                    $"\n\u2014 {rowStuff.def.defName}\n\u2003 ({rowStuff.def.modContentPack?.Name})"
                                );
                                break;
                            case StatKind.Base:
                                toolTip = (
                                    $"{rowStuff.def.label}\n{colStat.def.label}" +
                                    $"\n\nDebug:" +
                                    $"\n\u2014 {value:0.000000}" +
                                    $"\n\u2014 {rowStuff.def.defName}\n\u2003 ({rowStuff.def.modContentPack?.Name})" +
                                    $"\n\u2014 {colStat.def.defName}\n\u2003 ({colStat.def.modContentPack?.Name})"
                                );
                                break;
                            case StatKind.Factor:
                                toolTip = (
                                    $"{rowStuff.def.label}\n{colStat.def.label}" +
                                    $"\nfactor" +
                                    $"\n\nDebug:" +
                                    $"\n\u2014 {value:0.000000}" +
                                    $"\n\u2014 {rowStuff.def.defName}\n\u2003 ({rowStuff.def.modContentPack?.Name})" +
                                    $"\n\u2014 {colStat.def.defName}\n\u2003 ({colStat.def.modContentPack?.Name})"
                                );
                                break;
                            case StatKind.Offset:
                                toolTip = (
                                    $"{rowStuff.def.label}\n{colStat.def.label}" +
                                    $"\noffset" +
                                    $"\n\nDebug:" +
                                    $"\n\u2014 {value:0.000000}" +
                                    $"\n\u2014 {rowStuff.def.defName}\n\u2003 ({rowStuff.def.modContentPack?.Name})" +
                                    $"\n\u2014 {colStat.def.defName}\n\u2003 ({colStat.def.modContentPack?.Name})"
                                );
                                break;
                        }

                        this.CellTooltips[rowIndex, colIndex] = toolTip;
                    }
                }
            } finally {
                Text.Font = oldFont;
            }
        }

        public void DrawWindow(Rect inRect, ref Vector2 scrollPosition, Func<int, Action> makeSelectCategoryAction, Vector2 catSelectButtonOffset) {
            CommonDrawWindow.DrawWindow(this, inRect, ref scrollPosition, makeSelectCategoryAction, catSelectButtonOffset);
        }
    }
}
