using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class Verb_ArcSprayProjectileFix : Verb_ArcSprayProjectile
    {
        public override bool Available()
        {
            if (verbProps.consumeFuelPerBurst > 0f)
            {
                CompRefuelable compRefuelable = caster.TryGetComp<CompRefuelable>();
                if (compRefuelable != null && compRefuelable.Fuel < verbProps.consumeFuelPerBurst)
                {
                    return false;
                }
            }
            return base.Available();
        }
    }

    public class Projectile_LiquidFix : Projectile_Liquid
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (Rand.Chance(def.projectile.preExplosionSpawnChance))
            {
                Thing thing = ThingMaker.MakeThing(def.projectile.preExplosionSpawnThingDef);
                thing.stackCount = def.projectile.preExplosionSpawnThingCount;
                GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }
}
