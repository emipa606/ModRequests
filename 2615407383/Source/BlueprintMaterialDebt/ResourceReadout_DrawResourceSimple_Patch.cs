using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BlueprintMaterialDebt
{
    [HarmonyPatch(typeof(ResourceReadout), nameof(ResourceReadout.DrawResourceSimple), new Type[] { typeof(Rect), typeof(ThingDef) })]
    static class ResourceReadout_DrawResourceSimple_Patch
    {
        private static readonly MethodInfo DrawIcon = AccessTools.Method(typeof(ResourceReadout), "DrawIcon");

        static bool Prefix(ResourceReadout __instance, ref Rect rect, ref ThingDef thingDef)
        {
            if (!BlueprintMaterialDebt.SubtractResources)
            {
                return true;
            }

            DrawIcon.Invoke(__instance, new object[] { rect.x, rect.y, thingDef });

            rect.y += 2f;
            int count = Find.CurrentMap.resourceCounter.GetCount(thingDef);

            if (ResourceCounter_UpdateResourceCounts_Patch.neededAmounts.TryGetValue(thingDef, out int neededAmount))
            {
                count -= neededAmount;
            }

            Color previousColor = GUI.color;
            if (count < 0)
            {
                GUI.color = Color.yellow;
            }

            Widgets.Label(new Rect(34f, rect.y, rect.width - 34f, rect.height), count.ToStringCached());

            GUI.color = previousColor;

            return false;
        }
    }
}
