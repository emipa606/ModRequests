using HarmonyLib;
using Microsoft.SqlServer.Server;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace UniqueItems
{
    public class UniqueItemsSettings : ModSettings
    {
        private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();
        public static ThingFilter filter;
        public static Dictionary<string, int> uniqueThingsMaxCount = new Dictionary<string, int>();
        public static HashSet<string> allowedThings = new HashSet<string>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref allowedThings, "allowedThings", LookMode.Value);
            Scribe_Collections.Look(ref uniqueThingsMaxCount, "uniqueThingsMaxCount", LookMode.Value, LookMode.Value, ref stringKeys, ref intValues);
        }
        private List<string> stringKeys;
        private List<int> intValues;
        private Vector2 scrollPosition;
        int scrollHeightCount = 0;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 24);
            Widgets.Label(rect, "DM.AdjustUniqueItemsMaxCount".Translate());
            rect = new Rect(rect.x, rect.yMax, rect.width, rect.height);
            Widgets.Label(rect, "DM.ModSettingsSliderWarning".Translate());

            var filterRect = new Rect(rect.x, 100, rect.width, 24);
            Widgets.Label(filterRect, "DM.ModSettingsSelectItemsToBeUnique".Translate());
            ThingFilterUI_DrawQualityFilterConfigPatch.preventFromDrawing = true;
            filterRect = new Rect(filterRect.x, filterRect.yMax, filterRect.width, 225);
            ThingFilterUI.DoThingFilterConfigWindow(filterRect, thingFilterState, filter, 
                forceHiddenFilters: DefDatabase<SpecialThingFilterDef>.AllDefsListForReading, forceHideHitPointsConfig: true);
            ThingFilterUI_DrawQualityFilterConfigPatch.preventFromDrawing = false;

            var defs = uniqueThingsMaxCount.Keys.Select(x => DefDatabase<ThingDef>.GetNamedSilentFail(x)).OrderBy(x => x.label).ToList();
            var outRect = new Rect(rect.x, filterRect.yMax + 15, rect.width, 225);
            var viewRect = new Rect(rect.x, outRect.y, rect.width - 30, scrollHeightCount);
            scrollHeightCount = 0;
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var pos = outRect.y;
            for (var num = 0; num < defs.Count; num++)
            {
                var thingDef = defs[num];
                var value = uniqueThingsMaxCount[thingDef.defName];
                scrollHeightCount += 24;
                rect = new Rect(inRect.x, pos + (num * 24), inRect.width, 24);
                var rect2 = GenUI.Rounded(GenUI.LeftPart(rect, 0.5f));
                Rect rect3 = GenUI.Rounded(GenUI.LeftPart(GenUI.Rounded(GenUI.RightPart(rect, 0.3f)), 0.67f));
                Rect rect4 = GenUI.Rounded(GenUI.RightPart(rect, 0.05f));
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect2, thingDef.LabelCap);
                value = (int)Widgets.HorizontalSlider(rect3, value, 0, 999, true, null, null, null, -1f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect4, value.ToString());
                Text.Anchor = anchor;
                uniqueThingsMaxCount[thingDef.defName] = value;
            }
            Widgets.EndScrollView();
            Core.ApplySettings(true);
        }
    }

    [HarmonyPatch(typeof(ThingFilterUI), "DrawQualityFilterConfig")]
    public static class ThingFilterUI_DrawQualityFilterConfigPatch
    {
        public static bool preventFromDrawing;
        public static bool Prefix()
        {
            return !preventFromDrawing;
        }
    }
}
