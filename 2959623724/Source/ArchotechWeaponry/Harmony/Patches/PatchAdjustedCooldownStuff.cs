using System;
using ArchotechWeaponry.DefOfs;
using HarmonyLib;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(VerbProperties))]
    [HarmonyPatch("AdjustedCooldown", new Type[] {typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef)})]
    public class PatchAdjustedCooldownStuff
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, Pawn attacker, VerbProperties __instance)
        {
            if (attacker !=  null && attacker.equipment is Pawn_EquipmentTracker equipmentTracker &&
                               equipmentTracker.pawn.MentalStateDef == MentalStateDefOf.ArchotechWrath)
            {
                if (__instance.IsMeleeAttack)
                {
                    __result *= 0.6f;
                }
                else
                {
                    __result *= 0.3f;
                }
            }
        }
    }
}