using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.Widgets;

namespace Revolus.TextileStats {
    public class CommonDrawWindow {
        public static Vector2 SetupScrolling(Vector2 position, Vector2 outSize, Vector2 viewSize, Vector2 scrollPosition, bool scroll) {
            var outRect = new Rect(position, outSize);
            if (mouseOverScrollViewStack.Count > 0) {
                mouseOverScrollViewStack.Push(mouseOverScrollViewStack.Peek() && outRect.Contains(Event.current.mousePosition));
            } else {
                mouseOverScrollViewStack.Push(outRect.Contains(Event.current.mousePosition));
            }

            return GUI.BeginScrollView(
                outRect, scrollPosition, new Rect(new Vector2(), viewSize),
                scroll, scroll,
                scroll ? GUI.skin.horizontalScrollbar : GUIStyle.none,
                scroll ? GUI.skin.verticalScrollbar : GUIStyle.none
            );
        }

        public static void DrawWindow(
            ITableGenerator tableGenerator,
            Rect inRect,
            ref Vector2 scrollPosition,
            Func<int, Action> makeSelectCategoryAction,
            Vector2 catSelectButtonOffset
        ) {
            var columnLabels = tableGenerator.ColumnLabels;
            var rowStuffs = tableGenerator.RowData;
            var cellStrings = tableGenerator.CellStrings;
            var maxColumnWidths = tableGenerator.MaxColumnWidths;
            var cellSizes = tableGenerator.CellSizes;
            var cellTooltips = tableGenerator.CellTooltips;
            var categoryIndex = tableGenerator.CategoryIndex;
            var sortBy = tableGenerator.SortBy;
            var categories = tableGenerator.Categories;
            var columnLabelHeaders = tableGenerator.ColumnLabelHeaders;

            var rowCount = rowStuffs.Length;
            var colCount = columnLabels.Length;

            var imageColWidth = 4 + (24 + 4) * 2;
            var tableCellSize = new Vector2(100, 32);
            var nameCellSize = new Vector2(160, tableCellSize.y);
            var innerTableSize = new Vector2(nameCellSize.x + (colCount - 1) * tableCellSize.x, rowCount * tableCellSize.y);
            var fstHeadlineHeight = columnLabelHeaders is null ? 0 : 18;
            var sndHeadlineHeight = 18 * 3;
            var cellPadding = 5;
            var tableOffsetSize = new Vector2(imageColWidth, tableCellSize.y + cellPadding + fstHeadlineHeight + sndHeadlineHeight);
            var barWidth = 16;

            var firstVisibleRow = (int) (scrollPosition.y / tableCellSize.y);
            var countVisibleRows = (int) (innerTableSize.y / tableCellSize.y + 1);
            var lastVisibleRow = Math.Min(rowCount, firstVisibleRow + countVisibleRows);

            Rect addPadding(Rect r) => new Rect(r.x + cellPadding, r.y, r.width - 2 * cellPadding, r.height);

            int? newSortBy = null;
            var highlightColumns = new RangeInt(0, 0);

            // category selection button

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            var categoryOptions = new List<DropdownMenuElement<int>>();
            for (int catIndex = 0, catLength = categories.Length; catIndex < catLength; ++catIndex) {
                var menuElement = new DropdownMenuElement<int>();
                menuElement.payload = catIndex;
                menuElement.option = new FloatMenuOption(categories[catIndex], makeSelectCategoryAction(catIndex));
                categoryOptions.Add(menuElement);
            }
            Dropdown(
                rect: new Rect(
                    inRect.x + catSelectButtonOffset.x, inRect.y + catSelectButtonOffset.y,
                    tableCellSize.x * 2, tableCellSize.y
                ),
                target: 0,
                getPayload: v => v,
                menuGenerator: v => categoryOptions,
                buttonLabel: categories[categoryIndex]
            );

            // first headline row

            if (fstHeadlineHeight > 0) {
                SetupScrolling(
                    new Vector2(inRect.x + tableOffsetSize.x, inRect.y + tableOffsetSize.y - sndHeadlineHeight - fstHeadlineHeight),
                    new Vector2(inRect.width - tableOffsetSize.x - barWidth, inRect.height - tableOffsetSize.y),
                    new Vector2(innerTableSize.x, fstHeadlineHeight),
                    new Vector2(scrollPosition.x, 0),
                    false
                );
                try {
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    var doHighlight = false;
                    var colIndex = 0;
                    foreach (var columnLabelHeader in columnLabelHeaders) {
                        var slotRect = new Rect(
                            x: nameCellSize.x + (colIndex - 1) * tableCellSize.x,
                            y: 0,
                            width: tableCellSize.x * columnLabelHeader.columnSpan,
                            height: fstHeadlineHeight
                        );
                        if (colIndex == 0) {
                            slotRect.x += tableCellSize.x - nameCellSize.x;
                            slotRect.width += nameCellSize.x - tableCellSize.x;
                        }

                        if (doHighlight) {
                            DrawLightHighlight(slotRect);
                        }
                        doHighlight = !doHighlight;

                        if (Mouse.IsOver(slotRect)) {
                            DrawHighlight(slotRect);
                            highlightColumns = new RangeInt(colIndex, columnLabelHeader.columnSpan);
                        }

                        if (!string.IsNullOrEmpty(columnLabelHeader.optionalLabel)) {
                            Label(
                                new Rect(slotRect.x + cellPadding, slotRect.y, slotRect.width - 2 * cellPadding, slotRect.height),
                                columnLabelHeader.optionalLabel
                            );
                        }

                        colIndex += columnLabelHeader.columnSpan;
                    }
                } finally {
                    Text.Font = GameFont.Small;
                    EndScrollView();
                }
            }

            // second headline row

            SetupScrolling(
                new Vector2(inRect.x + tableOffsetSize.x, inRect.y + tableOffsetSize.y - sndHeadlineHeight),
                new Vector2(inRect.width - tableOffsetSize.x - barWidth, inRect.height - tableOffsetSize.y),
                new Vector2(innerTableSize.x, sndHeadlineHeight),
                new Vector2(scrollPosition.x, 0),
                false
            );
            try {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                for (var colIndex = 0; colIndex < colCount; ++colIndex) {
                    var cellOuterRect = new Rect(
                        position: new Vector2(nameCellSize.x + (colIndex - 1) * tableCellSize.x, 0),
                        size: new Vector2(tableCellSize.x, sndHeadlineHeight)
                    );
                    if (colIndex == 0) {
                        cellOuterRect.x += tableCellSize.x - nameCellSize.x;
                        cellOuterRect.width += nameCellSize.x - tableCellSize.x;
                    }
                    if (colIndex % 2 == 0) {
                        DrawLightHighlight(cellOuterRect);
                    }
                    if (colIndex >= highlightColumns.start && colIndex < highlightColumns.end) {
                        DrawHighlight(cellOuterRect);
                    } else if (Mouse.IsOver(cellOuterRect)) {
                        DrawHighlight(cellOuterRect);
                        highlightColumns = new RangeInt(colIndex, 1);
                    }

                    Texture2D image = null;
                    if (colIndex == sortBy) {
                        image = TexUI.ArrowTexLeft;
                    } else if (~colIndex == sortBy) {
                        image = TexUI.ArrowTexRight;
                    }
                    if (image != null) {
                        Color tmpColor = GUI.color;
                        try {
                            var r = new Rect(
                                x: cellOuterRect.x + (cellOuterRect.width - 50) / 2,
                                y: cellOuterRect.y + cellOuterRect.height * (1f / 6f) + cellPadding,
                                width: 50,
                                height: cellOuterRect.height * (2f / 3f) - cellPadding * 2
                            );
                            GUI.color = new Color(106f / 106f, 81f / 106f, 46f / 106f, 1f);
                            GUI.DrawTexture(r, image);
                            DrawShadowAround(r);
                        } finally {
                            GUI.color = tmpColor;
                        }
                    }

                    Label(addPadding(cellOuterRect), columnLabels[colIndex]);

                    if (ButtonInvisible(cellOuterRect)) {
                        if (sortBy == colIndex) {
                            newSortBy = ~colIndex;
                        } else if (sortBy == ~colIndex) {
                            newSortBy = colIndex;
                        } else if (colIndex == 0) {
                            newSortBy = colIndex;
                        } else {
                            newSortBy = ~colIndex;
                        }
                    }
                }
            } finally {
                Text.Font = GameFont.Small;
                EndScrollView();
            }

            // image column

            SetupScrolling(
                new Vector2(inRect.x, inRect.y + tableOffsetSize.y),
                new Vector2(imageColWidth, inRect.height - tableOffsetSize.y - barWidth),
                new Vector2(imageColWidth, innerTableSize.y),
                new Vector2(0, scrollPosition.y),
                false
            );
            try {
                for (var rowIndex = firstVisibleRow; rowIndex < lastVisibleRow; ++rowIndex) {
                    var rowStuff = rowStuffs[rowIndex];

                    var slotRect = new Rect(0, rowIndex * tableCellSize.y, innerTableSize.x, tableCellSize.y);
                    if (ButtonInvisible(slotRect)) {
                        Find.WindowStack.Add(new Dialog_InfoCard(rowStuff.def));
                    }
                    if (rowIndex % 2 == 0) {
                        DrawLightHighlight(slotRect);
                    }
                    DrawHighlightIfMouseover(slotRect);

                    MouseoverSounds.DoRegion(slotRect);
                    TooltipHandler.TipRegion(slotRect, $"{rowStuff.def.label}\n{"DefInfoTip".Translate()}");

                    var imageRect = new Rect(slotRect.x + 4, slotRect.y + 4, 24, 24);
                    ThingIcon(imageRect, rowStuff.def);

                    imageRect.x += imageRect.width + 4;
                    var colorTexture = rowStuff.colorTexture;
                    if (colorTexture != null) {
                        GUI.DrawTexture(imageRect, colorTexture);
                    }
                    GUI.DrawTexture(imageRect, UI.InfoButton);
                }
            } finally {
                EndScrollView();
            }

            // table cells

            scrollPosition = SetupScrolling(
                new Vector2(inRect.x + tableOffsetSize.x, inRect.y + tableOffsetSize.y),
                new Vector2(inRect.width - tableOffsetSize.x, inRect.height - tableOffsetSize.y),
                innerTableSize,
                scrollPosition,
                true
            );
            try {
                for (var rowIndex = firstVisibleRow; rowIndex < lastVisibleRow; ++rowIndex) {
                    var rowStuff = rowStuffs[rowIndex];

                    var slotRect = new Rect(0, rowIndex * tableCellSize.y, innerTableSize.x, tableCellSize.y);
                    if (rowIndex % 2 == 0) {
                        DrawLightHighlight(slotRect);
                    }
                    DrawHighlightIfMouseover(slotRect);

                    var nameOuterRect = new Rect(slotRect.position, nameCellSize);
                    var nameInnerRect = addPadding(nameOuterRect);
                    if (0 >= highlightColumns.start && 0 < highlightColumns.end) {
                        DrawHighlight(nameOuterRect);
                    }
                    DrawHighlightIfMouseover(nameOuterRect);
                    var labelString = rowStuff.def.label;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (Text.CalcHeight(labelString, nameInnerRect.width) > nameOuterRect.height) {
                        Text.Font = GameFont.Tiny;
                        Label(nameInnerRect, labelString);
                        Text.Font = GameFont.Small;
                    } else {
                        Label(nameInnerRect, labelString);
                    }
                    if (ButtonInvisible(nameOuterRect)) {
                        Find.WindowStack.Add(new Dialog_InfoCard(rowStuff.def));
                    }

                    TooltipHandler.TipRegion(nameOuterRect, labelString);

                    Text.Anchor = TextAnchor.MiddleRight;
                    for (var colIndex = 1; colIndex < colCount; ++colIndex) {
                        var cellString = cellStrings[rowIndex, colIndex];
                        var cellOuterRect = new Rect(
                            position: new Vector2(
                                x: slotRect.position.x + nameCellSize.x + (colIndex - 1) * tableCellSize.x,
                                y: slotRect.position.y
                            ),
                            size: tableCellSize
                        );

                        if (colIndex >= highlightColumns.start && colIndex < highlightColumns.end) {
                            DrawHighlight(cellOuterRect);
                        }

                        var cellInnerRect = addPadding(cellOuterRect);
                        DrawHighlightIfMouseover(cellOuterRect);
                        if (cellString is null) {
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Label(cellInnerRect, "\u2014");
                        } else if (cellSizes[rowIndex, colIndex].x <= 70) {
                            Text.Anchor = TextAnchor.MiddleRight;
                            var paddingLeft = Math.Max(0, (cellInnerRect.width - maxColumnWidths[colIndex]) / 2);
                            var alignedRect = new Rect(
                                position: new Vector2(cellInnerRect.x + paddingLeft, slotRect.y),
                                size: new Vector2(cellInnerRect.width - 2 * paddingLeft, slotRect.height)
                            );
                            Label(alignedRect, cellString);
                        } else {
                            Text.Anchor = TextAnchor.MiddleRight;
                            Text.Font = GameFont.Tiny;
                            Label(cellInnerRect, cellString);
                            Text.Font = GameFont.Small;
                        }

                        TooltipHandler.TipRegion(cellOuterRect, cellTooltips[rowIndex, colIndex]);
                    }
                }
            } finally {
                EndScrollView();
            }

            if (newSortBy != null) {
                tableGenerator.SortBy = (int) newSortBy;
                tableGenerator.ResetData();
            }
        }
    }
}
