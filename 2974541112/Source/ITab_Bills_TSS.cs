using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace zed_0xff.CPS;

public interface IBillTab
{
    BillStack BillStack { get; }
    Map Map { get; }
    IEnumerable<RecipeDef> GetAllRecipes();
    IntVec3 Position { get; }
}

public class ITab_Bills_TSS : ITab
{
    private float viewHeight = 1000f;
    private Vector2 scrollPosition;
    private Bill mouseoverBill;
    private static readonly Vector2 WinSize = new Vector2(420f, 480f);

    [TweakValue("Interface", 0f, 128f)]
    private static float PasteX = 48f;

    [TweakValue("Interface", 0f, 128f)]
    private static float PasteY = 3f;

    [TweakValue("Interface", 0f, 32f)]
    private static float PasteSize = 24f;

    public override bool IsVisible => SelThing is IBillTab;

    public ITab_Bills_TSS()
    {
        size = WinSize;
        labelKey = "TabBills";
    }

    protected IBillTab SelBuilding
    {
        get
        {
            return (IBillTab)SelThing;
        }
    }

    protected override void FillTab()
    {
        Rect rect2 = new Rect(WinSize.x - PasteX, PasteY, PasteSize, PasteSize);
        if (BillUtility.Clipboard != null)
        {
            if (!SelBuilding.GetAllRecipes().Contains(BillUtility.Clipboard.recipe) || !BillUtility.Clipboard.recipe.AvailableNow || !BillUtility.Clipboard.recipe.AvailableOnNow(SelThing))
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
                GUI.color = Color.white;
                if (Mouse.IsOver(rect2))
                {
                    TooltipHandler.TipRegion(rect2, "ClipboardBillNotAvailableHere".Translate() + ": " + BillUtility.Clipboard.LabelCap);
                }
            }
            else if (SelBuilding.BillStack.Count >= 15)
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
                GUI.color = Color.white;
                if (Mouse.IsOver(rect2))
                {
                    TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + "): " + BillUtility.Clipboard.LabelCap);
                }
            }
            else
            {
                if (Widgets.ButtonImageFitted(rect2, TexButton.Paste, Color.white))
                {
                    Bill bill = BillUtility.Clipboard.Clone();
                    bill.InitializeAfterClone();
                    SelBuilding.BillStack.AddBill(bill);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
                if (Mouse.IsOver(rect2))
                {
                    TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate() + ": " + BillUtility.Clipboard.LabelCap);
                }
            }
        }
        Rect rect3 = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            foreach (RecipeDef recipe in SelBuilding.GetAllRecipes()){
                if (recipe.AvailableNow) {
                    string label = recipe.LabelCap;
                    opts.Add(new FloatMenuOption(label, delegate
                                {
                                Bill bill = recipe.MakeNewBill();
                                SelBuilding.BillStack.AddBill(bill);
                                },
                                recipe.UIIconThing, null, forceBasicStyle: false, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe), null, playSelectionSound: true, -recipe.displayPriority));
                }
            }
            if (!opts.Any())
            {
                opts.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
            }
            return opts;
        };
        mouseoverBill = SelBuilding.BillStack.DoListing(rect3, recipeOptionsMaker, ref scrollPosition, ref viewHeight);
    }
}
