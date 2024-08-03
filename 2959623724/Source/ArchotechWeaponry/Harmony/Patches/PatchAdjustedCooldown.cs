using System;
using ArchotechWeaponry.DefOfs;
using HarmonyLib;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(VerbProperties))]
    [HarmonyPatch("AdjustedCooldown", new Type[] {typeof(Tool), typeof(Pawn), typeof(Thing)})]
    public class PatchAdjustedCooldown
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, Thing equipment, VerbProperties __instance)
        {
            if (equipment !=  null && equipment.ParentHolder is Pawn_EquipmentTracker equipmentTracker &&
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