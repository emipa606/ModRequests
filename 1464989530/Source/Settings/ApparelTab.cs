using UnityEngine;

using Verse;

namespace NightVision
{
    public class ApparelTab
    {
        private Vector2 _apparelScrollPosition = Vector2.zero;
        private const float CheckboxSize = 20f;

        public void Clear()
        {
            _apparelScrollPosition = Vector2.zero;
        }

        public void DrawTab(Rect inRect)
        {
            var nvApparel = Settings.Store.NVApparel;
            var cachedHeadgear = Settings.Cache.GetAllHeadgear;

            Text.Anchor = TextAnchor.LowerCenter;
            var apparelCount = cachedHeadgear.Count;
            var headerRect = new Rect(24f, 0f, inRect.width - 64f, 36f);
            var leftRect = headerRect.LeftPart(0.4f);
            var midRect = headerRect.RightPart(0.6f).LeftHalf().RightPart(0.8f);
            var rightRect = headerRect.RightPart(0.6f).RightHalf().LeftPart(0.8f);
            Widgets.Label(leftRect, "NVApparel".Translate());
            Widgets.Label(midRect, "NVNullPS".Translate());
            Widgets.Label(rightRect, "NVGiveNV".Translate());

            Widgets.DrawLineHorizontal(headerRect.x + 12f, headerRect.yMax + 4f, headerRect.xMax - 64f);

            Text.Anchor = TextAnchor.MiddleCenter;
            var viewRect = new Rect(32f, 48f, inRect.width - 64f, apparelCount * 48f);
            var scrollRect = new Rect(12f, 48f, inRect.width - 12f, inRect.height - 48f);


            var leftBoxX = midRect.center.x + CheckboxSize;
            var rightBoxX = rightRect.center.x + CheckboxSize;
            var leftBox = new Rect(leftBoxX, 0f, CheckboxSize, CheckboxSize);
            var rightBox = new Rect(rightBoxX, 0f, CheckboxSize, CheckboxSize);

            var num = 48f;
            Widgets.BeginScrollView(scrollRect, ref _apparelScrollPosition, viewRect);

            foreach (var apparelDef in cachedHeadgear)
            {
                var rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                Widgets.DrawAltRect(rowRect);

                var locGUIContent = new GUIContent(apparelDef.LabelCap, apparelDef.uiIcon);
                var apparelRect = rowRect.LeftPart(0.4f);
                Widgets.Label(apparelRect, locGUIContent);

                TooltipHandler.TipRegion(
                    apparelRect,
                    new TipSignal(apparelDef.DescriptionDetailed /*, apparelRect.GetHashCode()*/)
                );

                var leftBoxPos = new Vector2(leftBoxX, rowRect.center.y - CheckboxSize / 2);
                var rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - CheckboxSize / 2);
                leftBox.y = leftBoxPos.y;
                rightBox.y = leftBoxPos.y;
                TooltipHandler.TipRegion(leftBox, new TipSignal("PSApparelExplained".Translate()));
                TooltipHandler.TipRegion(rightBox, new TipSignal("NVApparelExplained".Translate()));

                if (nvApparel.TryGetValue(apparelDef, out var apparelSetting))
                {
                    Widgets.Checkbox(leftBoxPos, ref apparelSetting.NullifiesPS, CheckboxSize);
                    Widgets.Checkbox(rightBoxPos, ref apparelSetting.GrantsNV, CheckboxSize);

                    if (!apparelSetting.Equals(nvApparel[apparelDef]))
                    {
                        if (apparelSetting.IsRedundant())
                        {
                            nvApparel.Remove(apparelDef);
                        }
                        else
                        {
                            nvApparel[apparelDef] = apparelSetting;
                        }
                    }
                }
                else
                {
                    var nullPs = false;
                    var giveNV = false;
                    Widgets.Checkbox(leftBoxPos, ref nullPs, CheckboxSize);
                    Widgets.Checkbox(rightBoxPos, ref giveNV, CheckboxSize);

                    if (nullPs || giveNV)
                    {
                        if (apparelDef.GetCompProperties<CompProperties_NightVisionApparel>() is
                            CompProperties_NightVisionApparel compprops)
                        {
                            apparelSetting = compprops.AppVisionSetting;
                        }
                        else
                        {
                            apparelSetting =
                                ApparelVisionSetting.CreateNewApparelVisionSetting(apparelDef);
                        }

                        apparelSetting.NullifiesPS = nullPs;
                        apparelSetting.GrantsNV = giveNV;
                        nvApparel[apparelDef] = apparelSetting;
                    }
                }

                num += 48f;
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}