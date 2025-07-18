using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompAbilityEffect_ThunderField : CompAbilityEffect
{
    private new CompProperties_ThunderBurst Props => (CompProperties_ThunderBurst)props;

    private Pawn Pawn => parent.pawn;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        GenExplosion.DoExplosion(Pawn.Position, Pawn.MapHeld, Props.radius, DamageDefOf.ThunderWaveUlt, Pawn);
        base.Apply(target, dest);
    }

    public override IEnumerable<PreCastAction> GetPreCastActions()
    {
        yield return new PreCastAction
        {
            action = delegate { parent.AddEffecterToMaintain(EffecterDefOf.Thunder_Burst.Spawn(parent.pawn.Position, parent.pawn.Map), parent.pawn.Position, 17, parent.pawn.Map); },
            ticksAwayFromCast = 17
        };
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        Pawn pawn;
        return Pawn.Faction != Faction.OfPlayer && target.HasThing && (pawn = target.Thing as Pawn) != null && pawn.TargetCurrentlyAimingAt == Pawn;
    }
}