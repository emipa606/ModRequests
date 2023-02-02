using System;
using System.Reflection;
using HarmonyLib;
using ToggleableReadouts;
using UnityEngine;
using Verse;

namespace BlueprintMaterialDebt.ToggleableReadout
{
    [HarmonyPatch]
    static class ToggleableReadoutsUtility_DoThingDef_Patch
    {
        public static MethodBase TargetMethod()
        {
            return Type.GetType("ToggleableReadouts.ToggleableReadoutsUtility,ToggleableReadouts").GetMethod("DoThingDef", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void Prefix(ReadoutCache readout) // if this was capitalized the same way in both functions, there wouldn't need to be two identical patches
        {
            if (!BlueprintMaterialDebt.SubtractResources)
            {
                return;
            }

            var thingDef = (ThingDef)readout.def;
            if (ResourceCounter_UpdateResourceCounts_Patch.neededAmounts.TryGetValue(thingDef, out int neededAmount))
            {
                int count = readout.value - neededAmount;
                readout.valueLabel = count.ToStringCached();
                GUI.color = Color.yellow;
            }
        }

        public static void Postfix()
        {
            GUI.color = Color.white;
        }
    }
}
