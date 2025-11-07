using RimWorld;
using System;
using UnityEngine;
using Unity;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class ITab_DisplayShelf : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        private static Building_Locker locker;

        private static DisplayOffsetInfo baseDisplayInfo => locker.displayInfo;

        int index;

        bool bulk;

        bool Bulk => (bulk || KeyBindingDefOf.ModifierIncrement_10x.IsDown) && !(bulk && KeyBindingDefOf.ModifierIncrement_10x.IsDown);

        private DisplayOffsetInfo displayInfo => locker.DisplayInfoForIndex(index);

        DisplayOffsetInfo infoCache;

        private Building_Locker SelectedLocker
        {
            get
            {
                return Find.Selector.SingleSelectedThing as Building_Locker;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return SelectedLocker != null;// && SelectedLocker.isDisplay && SelectedLocker.tempStorage.Any;
            }
        }

        public ITab_DisplayShelf()
        {
            size = WinSize;
            labelKey = "BDshelves_DisplayTab";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            index = 0;
        }

        protected override void FillTab()
        {
            if (locker != SelectedLocker)
            {
                locker = SelectedLocker;
            }
            Rect rectWholeWindow = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Text.Font = GameFont.Tiny;

            if (locker.displayInfos != null && locker.displayInfos.Any())
            {
                rectWholeWindow.SplitHorizontally(30, out Rect switchRect, out rectWholeWindow);
                DrawSwitchPanel(switchRect);
            }

            FourCorners(rectWholeWindow, out Rect TL, out Rect TR, out Rect BL, out Rect BR);

            DrawControlPanel(TL);

            DrawControlPanelII(BL);

            Rect rect3 = TR.BottomPartPixels(50);
            if (Widgets.ButtonText(rect3.LeftHalf(), "BDshelves_Buttom_Copy".Translate(), true, true, true, null))
            {
                infoCache = displayInfo;
            }
            if (Widgets.ButtonText(rect3.RightHalf(), "BDshelves_Buttom_Paste".Translate(), true, true, true, null))
            {
                if (infoCache != null)
                {
                    displayInfo.Assign(infoCache);
                    locker.RefreshMaterial();
                }
            }
            Widgets.Label(TR.BottomPartPixels(TR.height - 10), displayInfo.ToString());

            Widgets.Label(BR.TopPartPixels(30), "BDshelves_Buttom_Description".Translate());
            baseDisplayInfo.description = Widgets.TextArea(BR.BottomPartPixels(BR.height - 30), baseDisplayInfo.description);
        }

        void FourCorners(Rect source, out Rect TL, out Rect TR, out Rect BL, out Rect BR)
        {
            Rect T = source.TopHalf();
            TL = T.LeftHalf();
            TR = T.RightHalf();
            T = source.BottomHalf();
            BL = T.LeftHalf();
            BR = T.RightHalf();
        }

        void DrawControlPanel(Rect source)
        {
            float width = Math.Min(source.width, source.height) / 3;

            Vector2 size = new Vector2(width, width);

            Vector2 currentPos = new Vector2(source.x, source.y);

            Vector2 right = new Vector2(width, 0);

            Vector2 down = new Vector2(0, width);

            //L A R
            //< B >
            //- V +
            if (Widgets.ButtonText(new Rect(currentPos, size), "L", true, true, true, null))
            {
                displayInfo.drawRot -= Bulk ? 12f : 1f; locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(new Rect(currentPos + right, size), "A", true, true, true, null))
            {
                displayInfo.drawOffset.z += Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(new Rect(currentPos + right * 2, size), "R", true, true, true, null))
            {
                displayInfo.drawRot += Bulk ? 12f : 1f; locker.Notify_ColorChanged();
            }
            currentPos += down;
            if (Widgets.ButtonText(new Rect(currentPos, size), "<", true, true, true, null))
            {
                displayInfo.drawOffset.x -= Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(new Rect(currentPos + right, size), Bulk ? "BDshelves_Buttom_bulk".Translate() : "BDshelves_Buttom_single".Translate(), true, true, true, null))
            {
                bulk = !bulk;
            }
            if (Widgets.ButtonText(new Rect(currentPos + right * 2, size), ">", true, true, true, null))
            {
                displayInfo.drawOffset.x += Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            currentPos += down;
            if (Widgets.ButtonText(new Rect(currentPos, size), "+", true, true, true, null))
            {
                displayInfo.drawSizeMult += Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(new Rect(currentPos + right, size), "V", true, true, true, null))
            {
                displayInfo.drawOffset.z -= Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(new Rect(currentPos + right * 2, size), "-", true, true, true, null))
            {
                displayInfo.drawSizeMult -= Bulk ? 0.1f : 0.01f; locker.Notify_ColorChanged();
            }
            currentPos += down;
            if (Widgets.ButtonText(new Rect(currentPos, new Vector2(3 * width, width)), "BDshelves_ShowInnerContainer".Translate(), true, true, true, null))
            {
                displayInfo.showInnerContainer = !displayInfo.showInnerContainer; locker.RefreshMaterial();
            }
        }

        void DrawControlPanelII(Rect source)
        {
            Rect rect = source.BottomHalf();

            if (Widgets.ButtonText(rect.BottomHalf(), "BDshelves_Buttom_Reset".Translate(), true, true, true, null))
            {
                displayInfo.drawOffset = new Vector3();
                displayInfo.drawRot = 0;
                displayInfo.drawSizeMult = 1;
                displayInfo.shouldFlip = false;
                locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(rect.LeftHalf().TopHalf(), "BDshelves_Buttom_flip".Translate(), true, true, true, null))
            {
                displayInfo.shouldFlip = !displayInfo.shouldFlip;
                locker.Notify_ColorChanged();
            }
            if (Widgets.ButtonText(rect.RightHalf().TopHalf(), "BDshelves_Buttom_changestack".Translate(), true, true, true, null))
            {
                displayInfo.useSingleText = !displayInfo.useSingleText;
                locker.RefreshMaterial();
            }


            Rect rect2 = source.TopHalf();
            if (Widgets.ButtonText(rect2.LeftHalf().TopHalf(), "BDshelves_Button_rotOverrride".Translate(displayInfo.rotOverride.IsValid ? displayInfo.rotOverride.ToStringHuman() : "N/A"), true, true, true, null))
            {
                displayInfo.CycleRotOverride();
                locker.RefreshMaterial();
            }
            if (Widgets.ButtonText(rect2.RightHalf().TopHalf(), "BDshelves_Buttom_hideTop".Translate(), true, true, true, null))
            {
                baseDisplayInfo.hideTop = !baseDisplayInfo.hideTop;
                locker.Notify_ColorChanged();
            }
        }

        void DrawSwitchPanel(Rect source)
        {
            Rect tempR = source.LeftPart(0.1f);
            TooltipHandler.TipRegion(tempR, "BDshelves_SwapInfoTip".Translate(KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel));
            if (Widgets.ButtonText(tempR, "<", true, true, true, null))
            {
                if (KeyBindingDefOf.ModifierIncrement_10x.IsDown)
                {
                    locker.MoveThingIndex(ref index, false);
                }
                else
                {
                    index--;
                    if (index < 0) index = locker.displayInfos.Count;
                }
            }

            tempR = source.RightPart(0.1f);
            TooltipHandler.TipRegion(tempR, "BDshelves_SwapInfoTip".Translate(KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel));
            if (Widgets.ButtonText(tempR, ">", true, true, true, null))
            {
                if (KeyBindingDefOf.ModifierIncrement_10x.IsDown)
                {
                    locker.MoveThingIndex(ref index, true);
                }
                else
                {
                    index++;
                    if (index > locker.displayInfos.Count) index = 0;
                }
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            string str = index < locker.tempStorage.Count ? $"({locker.tempStorage[index].Label})" : "";
            Widgets.Label(source, $"{index + 1}{str}");
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
