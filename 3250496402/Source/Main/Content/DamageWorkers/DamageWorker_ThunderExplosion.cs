using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class DamageWorker_ThunderExplosion : DamageWorker_AddInjury
{
    private const float VaporizeRadius = 2.9f;

    private static readonly FloatRange FireSizeRange = new(0.4f, 0.8f);

    static DamageWorker_ThunderExplosion()
    {
    }

    public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
    {
        var flag = c.DistanceTo(explosion.Position) <= 2.9f;
        var firstThing = c.GetFirstThing(explosion.Map, ThingDefOf.Filth_FireFoam);
        if (firstThing != null) firstThing.Destroy();
        base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes && flag);
        FireUtility.TryStartFireIn(c, explosion.Map, FireSizeRange.RandomInRange, explosion.instigator);
        if (flag) FleckMaker.ThrowSmoke(c.ToVector3Shifted(), explosion.Map, 2f);
    }

    protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
    {
        if (cell.DistanceTo(explosion.Position) <= 2.9f) base.ExplosionDamageThing(explosion, t, damagedThings, ignoredThings, cell);
    }

    public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
    {
        base.ExplosionStart(explosion, cellsToAffect);
        var effecter = EffecterDefOf.Thunder_Explosion.Spawn();
        effecter.Trigger(new TargetInfo(explosion.Position, explosion.Map), TargetInfo.Invalid);
        effecter.Cleanup();
    }
}