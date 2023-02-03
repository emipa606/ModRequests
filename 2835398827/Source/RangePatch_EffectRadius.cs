using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ShardMods
{
    [HarmonyPatch(typeof(AbilityDef), "EffectRadius", MethodType.Getter)]
    class RangePatch_EffectRadius
    { 
        static void Postfix(AbilityDef __instance, ref float __result)
        {
            if (__instance.cachedTooltipPawn == null)
                return;
            if (__instance.IsPsycast != true)
                return;
            if (__instance.label == "flashstorm" || __instance.label == "skipshield" || __instance.label == "solar pinhole" || __instance.label == "arctic pinhole" || __instance.label == "EMP pulse" || __instance.label == "flamebolt" || __instance.label == "fetility skip" || __instance.label == "haul skip" || __instance.label == "clean skip")
                return;
            __result *= __instance.cachedTooltipPawn.GetStatValue(StatDefOf.PsychicSensitivity);
            return;
        }
    }
}
