using Reload.Data;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Reload.UI
{
    [StaticConstructorOnStartup]
    public class Dialog_ManageStocks : Window
    {
        Stock _selStock;

        Stock SelectedStock
        {
            get
            {
                return _selStock;
            }
            set
            {
                CheckSelectedStockHasName();
                _selStock = value;
            }
        }

        Vector2 scrollPosition;

        static readonly Regex ValidNameRegex = Outfit.ValidNameRegex;

        public override Vector2 InitialSize => new Vector2(550f, 700f);

        public Dialog_ManageStocks(Stock selectedAssignedStock) 
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            SelectedStock = selectedAssignedStock;
        }

        void CheckSelectedStockHasName()
        {
            if (SelectedStock != null && SelectedStock.label.NullOrEmpty())
            {
                SelectedStock.label = "Unnamed";
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            float num = 0f;
            Rect rect = new Rect(0f, 0f, 150f, 35f);
            num += 150f;
            if (Widgets.ButtonText(rect, "SelectDrugPolicy".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Stock allPolicy in Base.Instance.StockDataStorage.AllStocks)
                {
                    Stock localAssignedDrugs2 = allPolicy;
                    list.Add(new FloatMenuOption(localAssignedDrugs2.label, delegate
                    {
                        SelectedStock = localAssignedDrugs2;
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            num += 10f;
            Rect rect2 = new Rect(num, 0f, 150f, 35f);
            num += 150f;
            if (Widgets.ButtonText(rect2, "NewDrugPolicy".Translate()))
            {
                SelectedStock = Base.Instance.StockDataStorage.MakeNewStock();
            }

            num += 10f;
            Rect rect3 = new Rect(num, 0f, 150f, 35f);
            num += 150f;
            if (Widgets.ButtonText(rect3, "DeleteDrugPolicy".Translate()))
            {
                List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                foreach (Stock allPolicy2 in Base.Instance.StockDataStorage.AllStocks)
                {
                    Stock localAssignedDrugs = allPolicy2;
                    list2.Add(new FloatMenuOption(localAssignedDrugs.label, delegate
                    {
                        AcceptanceReport acceptanceReport = Base.Instance.StockDataStorage.TryDelete(localAssignedDrugs);
                        if (!acceptanceReport.Accepted)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (localAssignedDrugs == SelectedStock)
                        {
                            SelectedStock = null;
                        }
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(list2));
            }

            Rect rect4 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - Window.CloseButSize.y).ContractedBy(10f);
            if (SelectedStock == null)
            {
                GUI.color = Color.grey;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, "NoStockPolicySelected".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            else
            {
                Widgets.BeginGroup(rect4);
                DoNameInputRect(new Rect(0f, 0f, 200f, 30f), ref SelectedStock.label);
                Rect rect5 = new Rect(0f, 40f, rect4.width, rect4.height - 45f - 10f);
                DoPolicyConfigArea(rect5);
                Widgets.EndGroup();
            }
        }
        public override void PreClose()
        {
            base.PreClose();
            CheckSelectedStockHasName();
        }
        public static void DoNameInputRect(Rect rect, ref string name)
        {
            name = Widgets.TextField(rect, name, 30, ValidNameRegex);
        }
        void DoPolicyConfigArea(Rect rect)
        {
            List<ThingDef> ammoDefs = Setting.AmmoDefs.Select(x=>(ThingDef)x).ToList();
            Rect rect2 = rect;
            rect2.height = 54f;
            Rect rect3 = rect;
            rect3.yMin = rect2.yMax;
            rect3.height -= 50f;
            Widgets.DrawMenuSection(rect3);

            float height = ammoDefs.Count * 35f;
            Rect viewRect = new Rect(0f, 0f, rect3.width - 16f, height);
            Widgets.BeginScrollView(rect3, ref scrollPosition, viewRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            for(int i = 0; i < ammoDefs.Count; i++)
            {
                Rect rect5 = new Rect(0f, (float)i * 35f, viewRect.width, 35f);
                GenUI.SplitVertically(rect5, rect5.height, out Rect rect_icon, out Rect rect_label);
                GenUI.SplitVertically(rect_label, rect_label.width * 0.5f, out rect_label, out Rect rect_input);
                float num = rect_input.height * 0.2f;
                rect_input.height -= num;
                rect_input.y += num / 2f;
                rect_input = rect_input.LeftHalf();
                Widgets.DrawHighlightIfMouseover(rect5);
                Widgets.DrawTextureFitted(rect_icon, Widgets.GetIconFor(ammoDefs[i]), 0.85f);
                Widgets.Label(rect_label, ammoDefs[i].label);
                int amount = SelectedStock.GetAmount(ammoDefs[i]);
                CustomWidgets.TextFieldInt(rect_input, ref amount, 0, 9999);
                SelectedStock.SetAmount(ammoDefs[i], amount);
            }
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.EndScrollView();
        }
    }
}
