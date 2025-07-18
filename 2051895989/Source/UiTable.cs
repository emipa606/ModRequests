using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace IncidentTweaker
{
    public class UiTable
    {
        int currentRow = 0;
        float rowHeight;
        float[] columnWidthsTemplate;
        string[] header;
        float[] columnOffsets;
        float[] columnWidths;
        Rect uiRect;
        Rect viewRect;
        Rect userRect;

        public UiTable(float rowHeight, float[] columnWidthsTemplate, string[] header)
        {
            this.rowHeight = rowHeight;
            this.columnWidthsTemplate = columnWidthsTemplate;
            this.header = header;

            columnOffsets = new float[columnWidthsTemplate.Length];
            columnWidths = new float[columnWidthsTemplate.Length];
        }
        public void Start(int rowCount, Rect rect)
        {
            Start(rowCount, rect.x, rect.y, rect.width, rect.height);
        }

        public void Start(int rowCount, float x, float y, float w, float h)
        {
            currentRow = 0;

            if (uiRect.x != x || uiRect.y != y || uiRect.width != w || uiRect.height != h)
            {
                if (header != null)
                {
                    y += rowHeight;
                    h -= rowHeight;
                }

                uiRect = new Rect(x, y, w, h);
                viewRect = new Rect(0, 0, w - 32f, rowCount * rowHeight);

                float totalNeededWidth = 0;
                float totalAvailableWidth = viewRect.width;
                foreach (float cw in columnWidthsTemplate)
                {
                    if (cw > 0)
                    {
                        totalNeededWidth += cw;
                    }
                    else
                    {
                        totalAvailableWidth += cw;
                    }
                }

                float xoff = 0;
                int n = 0;
                foreach (float cw in columnWidthsTemplate)
                {
                    float calculatedWidth;
                    if (cw > 0)
                    {
                        calculatedWidth = cw * totalAvailableWidth / totalNeededWidth;
                    }
                    else
                    {
                        calculatedWidth = -cw;
                    }

                    columnOffsets[n] = xoff;
                    columnWidths[n] = calculatedWidth;

                    xoff += calculatedWidth;
                    n++;
                }

            }

            if (header != null)
            {
                Verse.Text.Anchor = TextAnchor.MiddleCenter;
                for (int i = 0; i < header.Length; i++)
                {
                    Rect rect = Cell(i, 0);
                    rect.y += y - rowHeight;
                    Text(rect, header[i]);
                }

                Verse.Text.Anchor = TextAnchor.MiddleLeft;
            }


            Widgets.BeginScrollView(uiRect, ref scrollPosition, viewRect, true);
        }

        public Rect Cell(int x, int y)
        {
            if (x < 0 || x >= columnWidths.Length)
                throw new Exception("Bad cell coordinates: (" + x + "," + y + ")");

            /*
            if (x == 0 && y % 2 == 0) {
                userRect.x = 0;
                userRect.y = y * rowHeight;
                userRect.width = viewRect.width;
                userRect.height = rowHeight;
                Widgets.DrawAltRect(userRect);
            }*/

            userRect.x = columnOffsets[x];
            userRect.y = y * rowHeight;
            userRect.width = columnWidths[x];
            userRect.height = rowHeight;

            //            if ((x+ y) % 2 == 0) {
            //                Widgets.DrawAltRect(userRect);
            //            }

            return userRect;
        }

        public Rect WholeRow(int y)
        {
            userRect.x = columnOffsets[0];
            userRect.y = y * rowHeight;
            userRect.width = viewRect.width;
            userRect.height = rowHeight;

            return userRect;
        }
        public Rect WholeRow()
        {
            return WholeRow(currentRow);
        }

        public Rect Cell(int x)
        {
            return Cell(x, currentRow);
        }

        public void Text(Rect rect, string text, string tooltip = null)
        {
            string truncated = text.Truncate(rect.width, null);
            Widgets.Label(rect, truncated);

            string tip = "";
            if (text != truncated) tip += text + "\n";
            if (tooltip != null) tip += tooltip;
            if (tip.Length != 0) TooltipHandler.TipRegion(rect, delegate () { return tip; }, Mathf.FloorToInt(rect.y * 612 + rect.x));
        }

        public void Text(int x, int y, string text, string tooltip = null)
        {
            Text(Cell(x, y), text, tooltip);
        }
        public void Text(int x, string text, string tooltip = null)
        {
            Text(x, currentRow, text, tooltip);
        }

        public void NextRow()
        {
            currentRow++;
        }

        public void Stop()
        {
            Widgets.EndScrollView();
        }



        protected Vector2 scrollPosition = Vector2.zero;
    }
}
