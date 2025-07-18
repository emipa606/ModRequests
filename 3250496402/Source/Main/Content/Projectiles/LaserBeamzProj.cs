using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class LaserBeamzProj : Projectile
{
    private MoteDualAttached LaserMote;

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var laserMote = GetComp<CompLaserBeamz>().Props.laserMote;
        var centerVector = usedTarget.CenterVector3;
        base.Impact(hitThing, blockedByShield);
        var cell = centerVector.ToIntVec3();
        var v = centerVector - launcher.DrawPos;
        v.y = 0f;
        var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        if (hitThing != null)
        {
            Pawn pawn;
            var instigatorGuilty = (pawn = launcher as Pawn) == null || !pawn.Drafted;
            var dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
            if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f) pawn.stances.stagger.StaggerFor(95);
            if (def.projectile.extraDamages != null)
                foreach (var extraDamage in def.projectile.extraDamages)
                    if (Rand.Chance(extraDamage.chance))
                    {
                        var dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                        hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                    }
        }

        if (laserMote != null)
        {
            LaserMote = MoteMaker.MakeInteractionOverlay(laserMote, launcher, new TargetInfo(cell, map));
            LaserMote.exactPosition = launcher.Position.ToVector3Shifted();
            LaserMote.exactRotation = v.ToAngleFlat();
        }
    }

    public override void Tick()
    {
        if (intendedTarget.Thing != null) destination = intendedTarget.Thing.DrawPos;
        base.Tick();
    }
}