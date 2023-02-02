using System;
using System.Reflection;
using HarmonyLib;
using ToggleableReadouts;
using UnityEngine;
using Verse;

namespace BlueprintMaterialDebt.ToggleableReadout
{
    [HarmonyPatch]
    static class ToggleableReadoutsUtility_DrawResourceSimple_Patch
    {
        public static MethodBase TargetMethod()
        {
            return Type.GetType("ToggleableReadouts.ToggleableReadoutsUtility,ToggleableReadouts").GetMethod("DrawResourceSimple", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void Prefix(ReadoutCache readOut)
        {
            if (!BlueprintMaterialDebt.SubtractResources)
            {
                return;
            }

            var thingDef = (ThingDef)readOut.def;
            if (ResourceCounter_UpdateResourceCounts_Patch.neededAmounts.TryGetValue(thingDef, out int neededAmount))
            {
                int count = readOut.value - neededAmount;
                readOut.valueLabel = count.ToStringCached();
                GUI.color = Color.yellow;
            }
        }

        public static void Postfix()
        {
            GUI.color = Color.white;
        }
    }
}
