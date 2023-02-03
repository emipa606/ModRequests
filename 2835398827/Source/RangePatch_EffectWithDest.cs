using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace ShardMods
{
    [HarmonyPatch(typeof(CompAbilityEffect_WithDest))]
    [HarmonyPriority(Priority.Last)]
    class RangePatch_EffectWithDest
    {

        [HarmonyPrefix]
        [HarmonyPatch("DrawHighlight")]
        static bool DrawHighlight_Prefix(LocalTargetInfo target, CompAbilityEffect_WithDest __instance)
        {
            if ((__instance.GetType() != typeof(CompAbilityEffect_Teleport)) && (__instance.GetType() != typeof(CompAbilityEffect_WordOfLove)))
                return true;
            if ((double)__instance.Props.range > 0.0)
            {
                float temprange = __instance.Props.range;
                if (__instance.CasterPawn != null)
                        temprange *= __instance.CasterPawn.GetStatValue(StatDefOf.PsychicSensitivity);

                if ((double)temprange >= (double)(Find.CurrentMap.Size.x + Find.CurrentMap.Size.z) || (double)temprange >= (double)GenRadial.MaxRadialPatternRadius)
                    return false;

                if (__instance.Props.requiresLineOfSight)
                    GenDraw_Custom.DrawRadiusRing(__instance.selectedTarget.Cell, temprange, Color.white, (Func<IntVec3, bool>)(c => GenSight.LineOfSight(__instance.selectedTarget.Cell, c, __instance.parent.pawn.Map, false, (Func<IntVec3, bool>)null, 0, 0) && __instance.CanPlaceSelectedTargetAt((LocalTargetInfo)c)));
                else
                    GenDraw_Custom.DrawRadiusRing(__instance.selectedTarget.Cell, temprange);
                return false;
            }
            if (!target.IsValid)
                return false;
            GenDraw_Custom.DrawTargetHighlight(target);
            return false;
        }



        [HarmonyPrefix]
        [HarmonyPatch("CanHitTarget")]
        static bool CanHitTarget_Prefix(CompAbilityEffect_WithDest __instance, ref bool __result, LocalTargetInfo target)
        {
            if (__instance.CasterPawn != null)
            {
                if ((__instance.GetType() != typeof(CompAbilityEffect_Teleport)) && (__instance.GetType() != typeof(CompAbilityEffect_WordOfLove)))
                    return true;
                __result = (bool)(__instance.CanPlaceSelectedTargetAt(target) && (target.IsValid && ((double)__instance.Props.range <= 0.0 || (double)target.Cell.DistanceTo(__instance.selectedTarget.Cell) <= ((float)__instance.Props.range * (float)__instance.CasterPawn.GetStatValue(StatDefOf.PsychicSensitivity))) && (!__instance.Props.requiresLineOfSight || GenSight.LineOfSight(__instance.selectedTarget.Cell, target.Cell, __instance.parent.pawn.Map, false, (Func<IntVec3, bool>)null, 0, 0))));
                return false;
            }
            return true;
        }
    }
}