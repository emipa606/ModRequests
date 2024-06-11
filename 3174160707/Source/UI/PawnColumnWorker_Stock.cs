using Reload.Data;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Reload.UI
{
    public class PawnColumnWorker_Stock : PawnColumnWorker
    {
        public const int ButtonHeight = 32;

        public const int TopAreaHeight = 65;

        public override void DoHeader(Rect rect, PawnTable table)
        {
            if(!Setting.EnableAmmo) 
                return;

            base.DoHeader(rect, table);
            MouseoverSounds.DoRegion(rect);
            Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
            if (Widgets.ButtonText(rect2, "ManageStockPolicy".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManageStocks(null));
            }
            UIHighlighter.HighlightOpportunity(rect2, "ManageStockPoilcy");
        }
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!Setting.EnableAmmo)
                return;

            DrawCellButton(rect, pawn, table);
        }
        public override int GetMinWidth(PawnTable table)
        {
            if (!Setting.EnableAmmo)
                return 0;
            return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
        }
        public override int GetOptimalWidth(PawnTable table)
        {
            if (!Setting.EnableAmmo)
                return 0;
            return Mathf.Clamp(Mathf.CeilToInt(104f), GetMinWidth(table), GetMaxWidth(table));
        }
        public override int GetMinHeaderHeight(PawnTable table)
        {
            if (!Setting.EnableAmmo)
                return 0;
            return Mathf.Max(base.GetMinHeaderHeight(table), 65);
        }
        void DrawCellButton(Rect rect, Pawn pawn, PawnTable table)
        {
            int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
            int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
            float x = rect.x;
            Rect rect2 = new Rect(x, rect.y + 2f, num, rect.height - 4f);
            string text = Base.Instance.StockDataStorage.GetStock(pawn).label;

            Widgets.Dropdown<Pawn, Stock>(rect2, pawn, (Pawn p) => Base.Instance.StockDataStorage.GetStock(pawn), Button_GenerateMenu, text.Truncate(rect2.width), null, text, paintable: true);
            x += (float)num;
            x += 4f;
            Rect rect3 = new Rect(x, rect.y + 2f, num2, rect.height - 4f);
            if (Widgets.ButtonText(rect3, "AssignTabEdit".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManageStocks(Base.Instance.StockDataStorage.GetStock(pawn)));
            }
            UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignStocks");
            UIHighlighter.HighlightOpportunity(rect3, "ButtonAssignStocks");
            x += (float)num2;
        }
        static IEnumerable<Widgets.DropdownMenuElement<Stock>> Button_GenerateMenu(Pawn pawn)
        {
            foreach (Stock assignedStock in Base.Instance.StockDataStorage.AllStocks)
            {
                yield return new Widgets.DropdownMenuElement<Stock>
                {
                    option = new FloatMenuOption(assignedStock.label, delegate
                    {
                        Base.Instance.StockDataStorage.SetStock(pawn, assignedStock);
                    }),
                    payload = assignedStock
                };
            }
        }
    }
}