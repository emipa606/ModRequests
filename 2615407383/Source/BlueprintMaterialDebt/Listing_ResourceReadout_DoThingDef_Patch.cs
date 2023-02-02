using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BlueprintMaterialDebt
{
    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef", new Type[] { typeof(ThingDef), typeof(int) })]
    static class Listing_ResourceReadout_DoThingDef_Patch
	{
		static bool Prefix(Listing_ResourceReadout __instance, Map ___map, float ___nestIndentWidth, ref float ___curY, ref ThingDef thingDef, ref int nestLevel)
		{
			if (!BlueprintMaterialDebt.SubtractResources)
			{
				return true;
			}

			int count = ___map.resourceCounter.GetCount(thingDef);
			if (ResourceCounter_UpdateResourceCounts_Patch.neededAmounts.TryGetValue(thingDef, out int neededAmount))
			{
				count -= neededAmount;
			}

			if (count == 0)
			{
				return false;
			}

			Rect rect = new Rect(0f, ___curY, __instance.ColumnWidth, __instance.lineHeight);
			rect.xMin = (float)nestLevel * ___nestIndentWidth + 18f;
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}

			if (Mouse.IsOver(rect))
			{
				ThingDef thing = thingDef;
				TooltipHandler.TipRegion(rect, new TipSignal(() => thing.LabelCap + ": " + thing.description.CapitalizeFirst(), thingDef.shortHash));
			}

			Rect rect2 = new Rect(rect);
			rect2.width = rect2.height = 28f;
			rect2.y = rect.y + rect.height / 2f - rect2.height / 2f;
			Widgets.ThingIcon(rect2, thingDef);

			Rect rect3 = new Rect(rect);
			rect3.xMin = rect2.xMax + 6f;

			Color previousColor = GUI.color;
			if (count < 0)
			{
				GUI.color = Color.yellow;
			}

			Widgets.Label(rect3, count.ToStringCached());

			GUI.color = previousColor;

			___curY += __instance.lineHeight + __instance.verticalSpacing;

			return false;
		}

	}
}
