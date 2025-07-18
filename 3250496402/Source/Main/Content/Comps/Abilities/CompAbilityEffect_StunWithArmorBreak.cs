using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompAbilityEffect_StunWithArmorBreak : CompAbilityEffect_WithDuration
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        if (target.HasThing)
        {
            base.Apply(target, dest);
            var pawn = target.Thing as Pawn;
            if (pawn != null)
            {
                pawn.stances.stunner.StunFor(GetDurationSeconds(pawn).SecondsToTicks(), parent.pawn, false);
                pawn.health.AddHediff(HediffDefOf.ArmorBreakerEX);
            }
        }
    }
}