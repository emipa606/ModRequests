using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class CompPostPenetrationExplosive : ThingComp
    {
        public CompProperties_PostPenetrationExplosive Props => props as CompProperties_PostPenetrationExplosive;

        public virtual void Explode(Thing instigator, Map map, ThingDef weapon, Thing intendedTarget, List<Thing> ignoredThings = null)
        {
            if (Props.explosiveRadius > 0f && parent.def != null)
            {
                GenExplosion.DoExplosion(instigator: (instigator == null || (instigator.HostileTo(parent.Faction) && parent.Faction != Faction.OfPlayer)) ? parent : instigator, center: parent.PositionHeld, map: map, radius: Props.explosiveRadius, damType: Props.explosiveDamageType, damAmount: Props.damageAmountBase, armorPenetration: Props.armorPenetrationBase, explosionSound: Props.explosionSound, weapon: weapon, projectile: instigator.def, intendedTarget: intendedTarget, postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef, postExplosionSpawnChance: Props.postExplosionSpawnChance, postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount, postExplosionGasType: Props.postExplosionGasType, applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef, preExplosionSpawnChance: Props.preExplosionSpawnChance, preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount, chanceToStartFire: Props.chanceToStartFire, damageFalloff: Props.damageFalloff, direction: null, ignoredThings: ignoredThings, affectedAngle: null, doVisualEffects: Props.doVisualEffects, propagationSpeed: Props.propagationSpeed);
                parent.Destroy();
            }
        }
    }

    //I really don't want to copy this, but tynan hates accurate explosive weapons
    public class CompProperties_PostPenetrationExplosive : CompProperties
    {
        public float explosiveRadius = 1.9f;

        public DamageDef explosiveDamageType;

        public int damageAmountBase = -1;

        public float armorPenetrationBase = -1f;

        public ThingDef postExplosionSpawnThingDef;

        public float postExplosionSpawnChance;

        public int postExplosionSpawnThingCount = 1;

        public bool applyDamageToExplosionCellsNeighbors;

        public ThingDef preExplosionSpawnThingDef;

        public float preExplosionSpawnChance;

        public int preExplosionSpawnThingCount = 1;

        public float chanceToStartFire;

        public bool damageFalloff;

        public bool explodeOnKilled;

        public GasType? postExplosionGasType;

        public bool doVisualEffects = true;

        public float propagationSpeed = 1f;

        public float explosiveExpandPerStackcount;

        public float explosiveExpandPerFuel;

        public EffecterDef explosionEffect;

        public SoundDef explosionSound;

        public List<DamageDef> startWickOnDamageTaken;

        public float startWickHitPointsPercent = 0.2f;

        public IntRange wickTicks = new IntRange(140, 150);

        public float wickScale = 1f;

        public List<DamageDef> startWickOnInternalDamageTaken;

        public bool drawWick = true;

        public float chanceNeverExplodeFromDamage;

        public float destroyThingOnExplosionSize;

        public DamageDef requiredDamageTypeToExplode;

        public IntRange? countdownTicks;

        public string extraInspectStringKey;

        public List<WickMessage> wickMessages;

        public CompProperties_PostPenetrationExplosive()
        {
            compClass = typeof(CompPostPenetrationExplosive);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (explosiveDamageType == null)
            {
                explosiveDamageType = DamageDefOf.Bomb;
            }
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (parentDef.tickerType != TickerType.Normal)
            {
                yield return "CompExplosive requires Normal ticker type";
            }
        }
    }

}
