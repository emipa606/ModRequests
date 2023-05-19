using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace WorksController
{
    [StaticConstructorOnStartup]
    public abstract class WC_WorkGiverColumnWorker
    {
        protected abstract float HeaderMarginLeft { get; }

        protected abstract float ColumnMarginLeft { get; }

        protected abstract TextAnchor HeaderTextAnchor { get; }

        protected abstract TextAnchor CellTextAnchor { get; }

        protected abstract float OptimalWidthFixed { get; }

        protected virtual Color DefaultHeaderColor
        {
            get
            {
                return Color.white;
            }
        }

        protected virtual GameFont DefaultHeaderFont
        {
            get
            {
                return GameFont.Small;
            }
        }

        public virtual void DoHeader(Rect rect, WC_WorkGiverTable table)
        {
            if (!this.def.label.NullOrEmpty())
            {
                Text.Font = this.DefaultHeaderFont;
                GUI.color = this.DefaultHeaderColor;
                Text.Anchor = HeaderTextAnchor;
                Rect rect2 = rect;
                rect2.y += 3f;
                rect2.x += HeaderMarginLeft;
                Widgets.Label(rect2, this.def.LabelCap.Resolve().Truncate(rect.width, null));
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }
            else if (this.def.HeaderIcon != null)
            {
                Vector2 headerIconSize = this.def.HeaderIconSize;
                int num = (int)((rect.width - headerIconSize.x) / 2f);
                GUI.DrawTexture(new Rect(rect.x + (float)num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y).ContractedBy(2f), this.def.HeaderIcon);
            }
            if (table.SortingBy == this.def)
            {
                Texture2D texture2D = table.SortingDescending ? WC_WorkGiverColumnWorker.SortingDescendingIcon : WC_WorkGiverColumnWorker.SortingIcon;
                GUI.DrawTexture(new Rect(rect.xMax - (float)texture2D.width - 1f, rect.yMax - (float)texture2D.height - 1f, (float)texture2D.width, (float)texture2D.height), texture2D);
            }
            if (this.def.HeaderInteractable)
            {
                Rect interactableHeaderRect = this.GetInteractableHeaderRect(rect, table);
                if (Mouse.IsOver(interactableHeaderRect))
                {
                    Widgets.DrawHighlight(interactableHeaderRect);
                    string headerTip = this.GetHeaderTip(table);
                    if (!headerTip.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
                    }
                }
                if (Widgets.ButtonInvisible(interactableHeaderRect, true))
                {
                    this.HeaderClicked(rect, table);
                }
            }

        }

        public abstract void DoCell(Rect rect, WC_DataObject data, WC_WorkGiverTable table);

        public virtual int GetMinWidth(WC_WorkGiverTable table)
        {
            if (!this.def.label.NullOrEmpty())
            {
                Text.Font = this.DefaultHeaderFont;
                int result = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).x);
                Text.Font = GameFont.Small;
                if (this.def.widthPriority > 0)
                {
                    result = Math.Max(this.def.widthPriority, result);
                }
                return result;
            }
            if (this.def.HeaderIcon != null)
            {
                return Mathf.CeilToInt(this.def.HeaderIconSize.x);
            }
            return 1;
        }

        public virtual int GetMaxWidth(WC_WorkGiverTable table)
        {
            return 1000000;
        }

        public virtual int GetOptimalWidth(WC_WorkGiverTable table)
        {
            int optimalWidth = this.GetMinWidth(table);
            
            if (this.def.widthPriority > 0)
            {
                optimalWidth = Math.Max(this.def.widthPriority, optimalWidth);
            }
            return optimalWidth;
        }

        public virtual int GetMinCellHeight(WC_DataObject data)
        {
            return 30;
        }

        public virtual int GetMinHeaderHeight(WC_WorkGiverTable table)
        {
            if (!this.def.label.NullOrEmpty())
            {
                Text.Font = this.DefaultHeaderFont;
                int result = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).y);
                Text.Font = GameFont.Small;
                return result;
            }
            if (this.def.HeaderIcon != null)
            {
                return Mathf.CeilToInt(this.def.HeaderIconSize.y);
            }
            return 0;
        }

        public virtual int Compare(WC_DataObject a, WC_DataObject b)
        {
            return 0;
        }

        protected virtual Rect GetInteractableHeaderRect(Rect headerRect, WC_WorkGiverTable table)
        {
            float num = Mathf.Min(25f, headerRect.height);
            return new Rect(headerRect.x, headerRect.yMax - num, headerRect.width, num);
        }

        protected virtual void HeaderClicked(Rect headerRect, WC_WorkGiverTable table)
        {
            if (this.def.sortable && !Event.current.shift)
            {
                if (Event.current.button == 0)
                {
                    if (table.SortingBy != this.def)
                    {
                        table.SortBy(this.def, true);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        return;
                    }
                    if (table.SortingDescending)
                    {
                        table.SortBy(this.def, false);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        return;
                    }
                    table.SortBy(null, false);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    return;
                }
                else if (Event.current.button == 1)
                {
                    if (table.SortingBy != this.def)
                    {
                        table.SortBy(this.def, false);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        return;
                    }
                    if (table.SortingDescending)
                    {
                        table.SortBy(null, false);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                        return;
                    }
                    table.SortBy(this.def, true);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }
            }
        }

        protected virtual string GetHeaderTip(WC_WorkGiverTable table)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!this.def.headerTip.NullOrEmpty())
            {
                stringBuilder.Append(this.def.headerTip);
            }
            if (this.def.sortable)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("ClickToSortByThisColumn".Translate());
            }
            return stringBuilder.ToString();
        }

        public WC_WorkGiverColumnDef def;

        protected const int DefaultCellHeight = 30;

        private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

        private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

        private const int IconMargin = 2;

    }
}
