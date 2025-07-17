using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace THVanillaPatches.Patches
{
    public class DevourerDigestionPatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        protected override List<PatchInfo> Patches => new List<PatchInfo>()
        {
            new PatchInfo(AccessTools.Method(typeof(CompAbilityEffect_ConsumeLeap), nameof(CompAbilityEffect_ConsumeLeap.OnJumpCompleted)), Prefix: new HarmonyMethod(GetType(), nameof(DevourerOnJumpCompleted))),
            new PatchInfo(AccessTools.Method(typeof(CompDevourer), nameof(CompDevourer.GetDigestionTicks)), Postfix: new HarmonyMethod(GetType(), nameof(DevourerGetDigestionTicks)))
        };
        
        private static void DevourerGetDigestionTicks(ref int __result, CompDevourer __instance)
        {
            __result = (int)(__result / __instance.Pawn.health.capacities.GetLevel(THVanillaPatchesDefsOf.Metabolism));
        }
        
        private static bool DevourerOnJumpCompleted(CompAbilityEffect_ConsumeLeap __instance, IntVec3 origin,
            LocalTargetInfo target)
        {
            return !(__instance.parent.pawn.health.capacities.GetLevel(THVanillaPatchesDefsOf.Metabolism) <= 0.5f);
        }
    }
}