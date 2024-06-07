using HarmonyLib;
using Verse;
using RimWorld;

namespace ForceXenogermImlpantation
{
    [HarmonyPatch(typeof(Xenogerm), nameof(Xenogerm.PawnIdeoDisallowsImplanting))]
    static class Patch_ForceXenogermImlpantation
    {
        static void Postfix(Xenogerm __instance, ref bool __result, Pawn selPawn)
        {
            if( (selPawn.IsSlaveOfColony || selPawn.IsPrisonerOfColony) && __result ){
                __result = false;
            }
        }
    }
}
