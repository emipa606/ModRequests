using HarmonyLib;
using RimWorld;
using Verse;

namespace Renamon
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        private static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.Dead)
            {
                var morphHediff = __instance.health.hediffSet.GetFirstHediffOfDef(Renamon_DefOf.Renamon_PawnDuplication) as Hediff_MorphedPawn;
                if (morphHediff != null)
                {
                    __instance.health.RemoveHediff(morphHediff);
                    if (!morphHediff.oldPawn.Dead)
                    {
                        morphHediff.oldPawn.Kill(dinfo, exactCulprit);
                        morphHediff.oldPawn.Corpse?.Destroy();
                    }
                }
            }
        }
    }    
}
