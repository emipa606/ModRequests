using HarmonyLib;
using RimWorld;
using Verse;

namespace Renamon
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class Pawn_HealthTracker_MakeDowned_Patch
    {
        private static void Postfix(Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
        {
            if (___pawn.Downed)
            {
                var morphHediff = ___pawn.health.hediffSet.GetFirstHediffOfDef(Renamon_DefOf.Renamon_PawnDuplication) as Hediff_MorphedPawn;
                if (morphHediff != null)
                {
                    var map = ___pawn.MapHeld;
                    ___pawn.health.RemoveHediff(morphHediff);
                    if (morphHediff.oldPawn.Dead)
                    {
                        GenExplosion.DoExplosion(radius: 3, center: ___pawn.PositionHeld, map: map,
                            damType: DamageDefOf.EMP, instigator: morphHediff.oldPawn);
                        morphHediff.oldPawn.Corpse?.Destroy();
                        var component = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
                        component.stackCount = Rand.RangeInclusive(1, 4);
                        GenSpawn.Spawn(component, ___pawn.PositionHeld, map);
                        FleckMaker.ThrowDustPuff(___pawn.PositionHeld, map, 1f);
                    }
                }
            }
        }
    }
}
