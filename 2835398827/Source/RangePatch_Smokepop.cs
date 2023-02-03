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
    [HarmonyPatch(typeof(CompAbilityEffect_Smokepop))]
    [HarmonyPriority(Priority.Last)]
    class RangePatch_Smokepop
    {

        [HarmonyPrefix]
        [HarmonyPatch("Apply")]
        static bool Apply_Prefix(CompAbilityEffect_Smokepop __instance, LocalTargetInfo target, LocalTargetInfo dest)
        {
            ReversePatch_CompAbilityEffect.Apply(__instance, target, dest);
            GenExplosion.DoExplosion(target.Cell, __instance.parent.pawn.MapHeld, __instance.Props.smokeRadius * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity), DamageDefOf.Smoke, (Thing)null, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Thing)null, ThingDefOf.Gas_Smoke, 1f, 1, false, (ThingDef)null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>)null);
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch("DrawEffectPreview")]
        static bool DrawEffectPreview_Prefix(CompAbilityEffect_Smokepop __instance, LocalTargetInfo target)
        {
            GenDraw_Custom.DrawRadiusRing(target.Cell, __instance.Props.smokeRadius * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            return false;
        }
    }
}
