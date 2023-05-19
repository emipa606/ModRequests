using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WorksController
{
    public class WC_WorkGiverTable
    {

        public List<WC_WorkGiverColumnDef> ColumnsListForReading
        {
            get
            {
                return this.def.columns;
            }
        }

        public WC_WorkGiverColumnDef SortingBy
        {
            get
            {
                return this.sortByColumn;
            }
        }

        public bool SortingDescending
        {
            get
            {
                return this.SortingBy != null && this.sortDescending;
            }
        }

        public Vector2 Size
        {
            get
            {
                this.RecacheIfDirty();
                return this.cachedSize;
            }
        }

        public float HeightNoScrollbar
        {
            get
            {
                this.RecacheIfDirty();
                return this.cachedHeightNoScrollbar;
            }
        }

        public float HeaderHeight
        {
            get
            {
                this.RecacheIfDirty();
                return this.cachedHeaderHeight;
            }
        }

        public List<WC_DataObject> DatasListForReading
        {
            get
            {
                this.RecacheIfDirty();
                return this.cachedDatas;
            }
        }

        public WC_WorkGiverTable(WC_WorkGiverTableDef def, Func<IEnumerable<WC_DataObject>> datasGetter, int uiWidth, int uiHeight)
        {
            this.def = def;
            this.datasGetter = datasGetter;
            this.SetMinMaxSize(def.minWidth, uiWidth, 0, uiHeight);
            this.SetDirty();
        }

        public void WC_WorkGiverTableOnGUI(Vector2 position)
        {
            if (Event.current.type == EventType.Layout)
            {
                return;
            }
            this.RecacheIfDirty();
            float num = this.cachedSize.x - 16f;
            int num2 = 0;
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                int num3;
                if (i == this.def.columns.Count - 1)
                {
                    num3 = (int)(num - (float)num2);
                }
                else
                {
                    num3 = (int)this.cachedColumnWidths[i];
                }
                float headerWidth = (float)((int)position.x + num2);
                float headerWidthPriority = this.def.columns[i].widthPriority;
                Rect rect = new Rect(headerWidth, (float)((int)position.y), (float)num3, (float)((int)this.cachedHeaderHeight));
                this.def.columns[i].Worker.DoHeader(rect, this);
                num2 += num3;
            }
            Rect outRect = new Rect((float)((int)position.x), (float)((int)position.y + (int)this.cachedHeaderHeight), (float)((int)this.cachedSize.x), (float)((int)this.cachedSize.y - (int)this.cachedHeaderHeight));
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)((int)this.cachedHeightNoScrollbar - (int)this.cachedHeaderHeight));
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            int num4 = 0;
            for (int j = 0; j < this.cachedDatas.Count; j++)
            {
                num2 = 0;
                if ((float)num4 - this.scrollPosition.y + (float)((int)this.cachedRowHeights[j]) >= 0f && (float)num4 - this.scrollPosition.y <= outRect.height)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, (float)num4, viewRect.width);
                    GUI.color = Color.white;
                    if (!this.CanAssignData(this.cachedDatas[j]))
                    {
                        GUI.color = Color.gray;
                    }

                    float rowHeight = (float)((int)this.cachedRowHeights[j]);

                    Rect rect2 = new Rect(0f, (float)num4, viewRect.width, rowHeight);

                    if (Mouse.IsOver(rect2))
                    {
                        GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    }

                    WC_DataObject data = this.cachedDatas[j];
                    for (int k = 0; k < this.def.columns.Count; k++)
                    {
                        int num5;
                        if (k == this.def.columns.Count - 1)
                        {
                            num5 = (int)(num - (float)num2);
                        }
                        else
                        {
                            num5 = (int)this.cachedColumnWidths[k];
                        }
                        Rect rect3 = new Rect((float)num2, (float)num4, (float)num5, rowHeight);
                        this.def.columns[k].Worker.DoCell(rect3, this.cachedDatas[j], this);

                        ////test seqno
                        //if (k == 2)
                        //{
                        //    GameFont bkFont = Text.Font;
                        //    TextAnchor bkAnchor = Text.Anchor;
                        //    Text.Font = GameFont.Small;
                        //    Text.Anchor = TextAnchor.UpperLeft;
                        //    Text.WordWrap = false;
                        //    Widgets.Label(rect3, j.ToString());
                        //    Text.WordWrap = true;
                        //    Text.Font = bkFont;
                        //    Text.Anchor = bkAnchor;
                        //}
                        ////test

                        num2 += num5;
                    }
                    GUI.color = Color.white;
                }
                num4 += (int)this.cachedRowHeights[j];
            }
            Widgets.EndScrollView();
        }

        public void SetDirty()
        {
            this.dirty = true;
        }

        public void SetMinMaxSize(int minTableWidth, int maxTableWidth, int minTableHeight, int maxTableHeight)
        {
            this.minTableWidth = minTableWidth;
            this.maxTableWidth = maxTableWidth;
            this.minTableHeight = minTableHeight;
            this.maxTableHeight = maxTableHeight;
            this.hasFixedSize = false;
            this.SetDirty();
        }

        public void SetFixedSize(Vector2 size)
        {
            this.fixedSize = size;
            this.hasFixedSize = true;
            this.SetDirty();
        }

        public void SortBy(WC_WorkGiverColumnDef column, bool descending)
        {
            this.sortByColumn = column;
            this.sortDescending = descending;
            this.SetDirty();
        }

        protected virtual bool CanAssignData(WC_DataObject data)
        {
            return true;
        }

        private void RecacheIfDirty()
        {
            if (!this.dirty)
            {
                return;
            }
            this.dirty = false;
            this.RecacheDatas();
            this.RecacheRowHeights();
            this.cachedHeaderHeight = this.CalculateHeaderHeight();
            this.cachedHeightNoScrollbar = this.CalculateTotalRequiredHeight();
            this.RecacheSize();
            this.RecacheColumnWidths();
        }

        private void RecacheDatas()
        {
            this.cachedDatas.Clear();
            this.cachedDatas.AddRange(this.datasGetter());
            this.cachedDatas = this.LabelSortFunction(this.cachedDatas).ToList<WC_DataObject>();
            if (this.sortByColumn != null)
            {
                if (this.sortDescending)
                {
                    this.cachedDatas.SortStable(new Func<WC_DataObject, WC_DataObject, int>(this.sortByColumn.Worker.Compare));
                }
                else
                {
                    this.cachedDatas.SortStable((WC_DataObject a, WC_DataObject b) => this.sortByColumn.Worker.Compare(b, a));
                }
            }
            this.cachedDatas = this.PrimarySortFunction(this.cachedDatas).ToList<WC_DataObject>();
        }

        protected virtual IEnumerable<WC_DataObject> LabelSortFunction(IEnumerable<WC_DataObject> input)
        {
            return from data in input
                   orderby data.PrimarySkillLabel, data.WorkGiverLabel
                   select data;
        }

        protected virtual IEnumerable<WC_DataObject> PrimarySortFunction(IEnumerable<WC_DataObject> input)
        {
            return input;
        }

        private void RecacheColumnWidths()
        {
            float num = this.cachedSize.x - 16f;
            float num2 = 0f;
            this.RecacheColumnWidths_StartWithMinWidths(out num2);
            if (num2 == num)
            {
                return;
            }
            if (num2 > num)
            {
                this.SubtractProportionally(num2 - num, num2);
                return;
            }
            bool flag;
            this.RecacheColumnWidths_DistributeUntilOptimal(num, ref num2, out flag);
            if (flag)
            {
                return;
            }
            this.RecacheColumnWidths_DistributeAboveOptimal(num, ref num2);
        }

        private void RecacheColumnWidths_StartWithMinWidths(out float minWidthsSum)
        {
            minWidthsSum = 0f;
            this.cachedColumnWidths.Clear();
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                float minWidth = this.GetMinWidth(this.def.columns[i]);
                this.cachedColumnWidths.Add(minWidth);
                minWidthsSum += minWidth;
            }
        }

        private void RecacheColumnWidths_DistributeUntilOptimal(float totalAvailableSpaceForColumns, ref float usedWidth, out bool noMoreFreeSpace)
        {
            this.columnAtOptimalWidth.Clear();
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                this.columnAtOptimalWidth.Add(this.cachedColumnWidths[i] >= this.GetOptimalWidth(this.def.columns[i]));
            }
            int num = 0;
            for (;;)
            {
                num++;
                if (num >= 10000)
                {
                    break;
                }
                float num2 = float.MinValue;
                for (int j = 0; j < this.def.columns.Count; j++)
                {
                    if (!this.columnAtOptimalWidth[j])
                    {
                        num2 = Mathf.Max(num2, (float)this.def.columns[j].widthPriority);
                    }
                }
                float num3 = 0f;
                for (int k = 0; k < this.cachedColumnWidths.Count; k++)
                {
                    if (!this.columnAtOptimalWidth[k] && (float)this.def.columns[k].widthPriority == num2)
                    {
                        num3 += this.GetOptimalWidth(this.def.columns[k]);
                    }
                }
                float num4 = totalAvailableSpaceForColumns - usedWidth;
                bool flag = false;
                bool flag2 = false;
                for (int l = 0; l < this.cachedColumnWidths.Count; l++)
                {
                    if (!this.columnAtOptimalWidth[l])
                    {
                        if ((float)this.def.columns[l].widthPriority != num2)
                        {
                            flag = true;
                        }
                        else
                        {
                            float num5 = num4 * this.GetOptimalWidth(this.def.columns[l]) / num3;
                            float num6 = this.GetOptimalWidth(this.def.columns[l]) - this.cachedColumnWidths[l];
                            if (num5 >= num6)
                            {
                                num5 = num6;
                                this.columnAtOptimalWidth[l] = true;
                                flag2 = true;
                            }
                            else
                            {
                                flag = true;
                            }
                            if (num5 > 0f)
                            {
                                List<float> list = this.cachedColumnWidths;
                                int index = l;
                                list[index] += num5;
                                usedWidth += num5;
                            }
                        }
                    }
                }
                if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
                {
                    goto Block_13;
                }
                if (!flag || !flag2)
                {
                    goto IL_243;
                }
            }
            Log.Error("Too many iterations.", false);
            goto IL_243;
            Block_13:
            noMoreFreeSpace = true;
            IL_243:
            noMoreFreeSpace = false;
        }

        private void RecacheColumnWidths_DistributeAboveOptimal(float totalAvailableSpaceForColumns, ref float usedWidth)
        {
            this.columnAtMaxWidth.Clear();
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                this.columnAtMaxWidth.Add(this.cachedColumnWidths[i] >= this.GetMaxWidth(this.def.columns[i]));
            }
            int num = 0;
            for (;;)
            {
                num++;
                if (num >= 10000)
                {
                    break;
                }
                float num2 = 0f;
                for (int j = 0; j < this.def.columns.Count; j++)
                {
                    if (!this.columnAtMaxWidth[j])
                    {
                        num2 += Mathf.Max(this.GetOptimalWidth(this.def.columns[j]), 1f);
                    }
                }
                float num3 = totalAvailableSpaceForColumns - usedWidth;
                bool flag = false;
                for (int k = 0; k < this.def.columns.Count; k++)
                {
                    if (!this.columnAtMaxWidth[k])
                    {
                        float num4 = num3 * Mathf.Max(this.GetOptimalWidth(this.def.columns[k]), 1f) / num2;
                        float num5 = this.GetMaxWidth(this.def.columns[k]) - this.cachedColumnWidths[k];
                        if (num4 >= num5)
                        {
                            num4 = num5;
                            this.columnAtMaxWidth[k] = true;
                        }
                        else
                        {
                            flag = true;
                        }
                        if (num4 > 0f)
                        {
                            List<float> list = this.cachedColumnWidths;
                            int index = k;
                            list[index] += num4;
                            usedWidth += num4;
                        }
                    }
                }
                if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
                {
                    return;
                }
                if (!flag)
                {
                    goto Block_10;
                }
            }
            Log.Error("Too many iterations.", false);
            return;
            Block_10:
            this.DistributeRemainingWidthProportionallyAboveMax(totalAvailableSpaceForColumns - usedWidth);
        }

        private void RecacheRowHeights()
        {
            this.cachedRowHeights.Clear();
            for (int i = 0; i < this.cachedDatas.Count; i++)
            {
                this.cachedRowHeights.Add(this.CalculateRowHeight(this.cachedDatas[i]));
            }
        }

        private void RecacheSize()
        {
            if (this.hasFixedSize)
            {
                this.cachedSize = this.fixedSize;
                return;
            }
            float num = 0f;
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                if (!this.def.columns[i].ignoreWhenCalculatingOptimalTableSize)
                {
                    num += this.GetOptimalWidth(this.def.columns[i]);
                }
            }
            float num2 = Mathf.Clamp(num + 16f, (float)this.minTableWidth, (float)this.maxTableWidth);
            float num3 = Mathf.Clamp(this.cachedHeightNoScrollbar, (float)this.minTableHeight, (float)this.maxTableHeight);
            num2 = Mathf.Min(num2, (float)UI.screenWidth);
            num3 = Mathf.Min(num3, (float)UI.screenHeight);
            this.cachedSize = new Vector2(num2, num3);
        }

        private void SubtractProportionally(float toSubtract, float totalUsedWidth)
        {
            for (int i = 0; i < this.cachedColumnWidths.Count; i++)
            {
                List<float> list = this.cachedColumnWidths;
                int index = i;
                list[index] -= toSubtract * this.cachedColumnWidths[i] / totalUsedWidth;
            }
        }

        private void DistributeRemainingWidthProportionallyAboveMax(float toDistribute)
        {
            float num = 0f;
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                num += Mathf.Max(this.GetOptimalWidth(this.def.columns[i]), 1f);
            }
            for (int j = 0; j < this.def.columns.Count; j++)
            {
                List<float> list = this.cachedColumnWidths;
                int index = j;
                list[index] += toDistribute * Mathf.Max(this.GetOptimalWidth(this.def.columns[j]), 1f) / num;
            }
        }

        private float GetOptimalWidth(WC_WorkGiverColumnDef column)
        {
            return Mathf.Max((float)column.Worker.GetOptimalWidth(this), 0f);
        }

        private float GetMinWidth(WC_WorkGiverColumnDef column)
        {
            return Mathf.Max((float)column.Worker.GetMinWidth(this), 0f);
        }

        private float GetMaxWidth(WC_WorkGiverColumnDef column)
        {
            return Mathf.Max((float)column.Worker.GetMaxWidth(this), 0f);
        }

        private float CalculateRowHeight(WC_DataObject data)
        {
            float num = 0f;
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                num = Mathf.Max(num, (float)this.def.columns[i].Worker.GetMinCellHeight(data));
            }
            num *= data.SkillDefs.Count;
            return num;
        }

        private float CalculateHeaderHeight()
        {
            float num = 0f;
            for (int i = 0; i < this.def.columns.Count; i++)
            {
                num = Mathf.Max(num, (float)this.def.columns[i].Worker.GetMinHeaderHeight(this));
            }
            return num;
        }

        private float CalculateTotalRequiredHeight()
        {
            float num = this.CalculateHeaderHeight();
            for (int i = 0; i < this.cachedDatas.Count; i++)
            {
                num += this.CalculateRowHeight(this.cachedDatas[i]);
            }
            return num;
        }

        private WC_WorkGiverTableDef def;

        private Func<IEnumerable<WC_DataObject>> datasGetter;

        private int minTableWidth;

        private int maxTableWidth;

        private int minTableHeight;

        private int maxTableHeight;

        private Vector2 fixedSize;

        private bool hasFixedSize;

        private bool dirty;

        private List<bool> columnAtMaxWidth = new List<bool>();

        private List<bool> columnAtOptimalWidth = new List<bool>();

        private Vector2 scrollPosition;

        private WC_WorkGiverColumnDef sortByColumn;

        private bool sortDescending;

        private Vector2 cachedSize;

        private List<WC_DataObject> cachedDatas = new List<WC_DataObject>();

        private List<float> cachedColumnWidths = new List<float>();

        private List<float> cachedRowHeights = new List<float>();

        private float cachedHeaderHeight;

        private float cachedHeightNoScrollbar;

    }
}
