using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace BDsPlasmaWeaponVanilla
{
    public class DamageWorker_LizionExplosion : DamageWorker_AddInjury
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            if (explosion.Map == Find.CurrentMap && explosion.radius > 4)
            {
                float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
            }
            ExplosionVisualEffectCenter(explosion);
        }

        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            if (t is Filth)
            {
                t.Destroy();
            }
            if (!(t is Pawn || t is Plant || t is Fire))
            {
                return;
            }
            base.ExplosionDamageThing(explosion, t, damagedThings, ignoredThings, cell);
        }
        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
            float num2 = 1f - lengthHorizontal / explosion.radius;
            if (num2 > 0f)
            {
                explosion.Map.snowGrid.AddDepth(c, (0f - num2) * def.explosionSnowMeltAmount);
            }
        }

        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult result = new DamageResult();
            if (victim is Pawn || victim is Plant)
            {
                return base.Apply(dinfo, victim);
            }
            if (victim is Fire)
            {
                victim.Destroy();
            }
            return result;
        }
    }
}
