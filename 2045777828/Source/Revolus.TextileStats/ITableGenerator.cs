using System;
using UnityEngine;

namespace Revolus.TextileStats {
    public interface ITableGenerator {
        int CategoryIndex {
            get;
            set;
        }

        int SortBy {
            get;
            set;
        }

        void ResetData();

        void DrawWindow(Rect inRect, ref Vector2 scrollPosition, Func<int, Action> makeSelectCategoryAction, Vector2 catSelectButtonOffset);
        
        string[,] CellStrings {
            get;
        }

        string[,] CellTooltips {
            get;
        }

        Vector2[,] CellSizes {
            get;
        }

        int[] MaxColumnWidths {
            get;
        }

        RowData[] RowData {
            get;
        }

        ColumnLabelHeader[] ColumnLabelHeaders {
            get;
        }

        string[] ColumnLabels {
            get;
        }

        string[] Categories {
            get;
        }
    }
}
