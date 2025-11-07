using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class PenetratingBullet : Bullet
    {
        public float PenetratingPowerBase => def.projectile.GetArmorPenetration(weaponDamageMultiplier) * PenetrationFloorPercentage;
        public override float ArmorPenetration => Extension == null ? def.projectile.GetArmorPenetration(weaponDamageMultiplier) : def.projectile.GetArmorPenetration(weaponDamageMultiplier) * (penetratingPower / MaxPenetratingPower);

        public bool ShouldTerminate;

        Vector3 initialVector;

        IntVec3 cachedPosition;

        Vector3 trueOrigin;

        ModExtension_PenetratingProjectile Extension => def.GetModExtension<ModExtension_PenetratingProjectile>();

        public override int DamageAmount => ShouldTerminate ? def.projectile.GetDamageAmount(StopperDamageMulti) : def.projectile.GetDamageAmount(overpenDamageMulti);
        public float overpenDamageMulti => (Extension == null) ? 0.5f : Extension.overpenDamageMulti;
        public float StopperDamageMulti => (Extension == null) ? 1f : Extension.stopperDamageMulti;
        public float PenetrationFloorPercentage => (Extension == null) ? 0.2f : Extension.PenetrationFloorPercentage;
        public float BuildingEquivalentMulti => (Extension == null) ? 0.005f : Extension.buildingEquivalentMulti;
        public float BodysizeEquivalentMulti => (Extension == null) ? 0.1f : Extension.bodysizeEquivalentMulti;
        public float TreeEquivalent => (Extension == null) ? 0.1f : Extension.treeEquivalent;
        public float ChunkEquivalent => (Extension == null) ? 0.25f : Extension.chunkEquivalent;
        public float BounceEquivalent => (Extension == null) ? 0.2f : Extension.bounceEquivalent;
        public float ArmorEquivalentMulti => (Extension == null) ? 1f : Extension.armorEquivalentMulti;
        public float MinSearchRadius => (Extension == null) ? 5f : Extension.minSearchRadius;
        public float ExtraRange => (Extension == null) ? 5f : Extension.extraRange;
        public float MaxSearchRadius => (Extension == null) ? 20f : Extension.maxSearchRadius;
        public float PostPenetrationDeviationAngle => (Extension == null) ? 5f : Extension.postPenetrationDeviationAngle;

        public float MaxPenetratingPower = 0;

        public float ticksToDetonation = 100;

        public bool fuseActivated = false;
        public CompPostPenetrationExplosive compExplosive => this.TryGetComp<CompPostPenetrationExplosive>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (def.projectile.damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
            {
                penetratingPower = def.projectile.GetArmorPenetration(weaponDamageMultiplier);
                if (Extension != null)
                {
                    penetratingPower *= Extension.penetrationPotentialMultiplier;
                }
            }
            MaxPenetratingPower = penetratingPower;
            if (compExplosive != null)
            {
                ticksToDetonation = compExplosive.Props.wickTicks.RandomInRange;
            }
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (equipment is Building_TurretGun turret)
            {
                equipment = turret.gun;
                attackedThings.Add(turret);
            }
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            initialVector = usedTarget.CenterVector3 - origin;
            initialVector.y = 0;
            trueOrigin = origin;
        }

        public override void Tick()
        {
            if (penetratingPower <= PenetratingPowerBase || ticksToImpact < 0 || Map == null)
            {
                Destroy();
                return;
            }
            if (landed == true)
            {
                landed = false;
            }
            if (Position != cachedPosition)
            {
                CheckForFreeIntercept(Position);
                cachedPosition = Position;
            }
            if (Map != null)
            {
                base.Tick();
            }
            if (compExplosive != null && fuseActivated)
            {
                ticksToDetonation--;
                if (ticksToDetonation < 0 || penetratingPower <= PenetratingPowerBase)
                {
                    compExplosive.Explode(this, this.Map, this.equipmentDef, this.intendedTarget.Thing);
                }
            }
        }

        List<Thing> thingList = new List<Thing>();
        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (destination.ToIntVec3() == c)
            {
                return false;
            }
            thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!CanHit(thing))
                {
                    continue;
                }
                bool flag2 = false;
                if (thing.def.Fillage == FillCategory.Full)
                {
                    if (!(thing is Building_Door building_Door) || !building_Door.Open)
                    {
                        Impact(thing);
                        return true;
                    }
                    flag2 = true;
                }
                float num2 = 0f;
                if (thing is Pawn pawn)
                {
                    num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0)
                    {
                        num2 *= 0.1f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        if (preventFriendlyFire)
                        {
                            num2 = 0f;
                        }
                        else
                        {
                            num2 *= Find.Storyteller.difficulty.friendlyFireChanceFactor * VerbUtility.InterceptChanceFactorFromDistance(origin, c);
                        }
                    }
                }
                else if (thing.def.fillPercent > 0.2f)
                {
                    num2 = (flag2 ? 0.05f : ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * 0.15f) : (thing.def.fillPercent * 1f)));
                }
                if (num2 > 1E-05f)
                {
                    if (Rand.Chance(num2))
                    {
                        Impact(thing);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (blockedByShield)
            {
                Destroy();
                return;
            }
            if (attackedThings.Contains(hitThing) || penetratingPower <= PenetratingPowerBase)
            {
                landed = false;
                return;
            }
            canDestroyNow = false;
            float removeValue = 0;
            if (hitThing is Building)
            {
                Effecter effect = RimWorld.EffecterDefOf.DamageDiminished_Metal.Spawn();
                effect.Trigger(hitThing, hitThing);
                effect.Trigger(hitThing, hitThing);
                if (hitThing.def.useHitPoints)
                {
                    removeValue = hitThing.HitPoints * BuildingEquivalentMulti;
                }
                else
                {
                    penetratingPower = 0;
                }
            }
            if (hitThing is Pawn pawn)
            {
                removeValue = GetPawnArmor(pawn) * 2f * ArmorEquivalentMulti + (float)(Math.Sqrt(pawn.BodySize) * pawn.BodySize) * BodysizeEquivalentMulti;
                EffecterDef damageEffecter = pawn.RaceProps.FleshType.damageEffecter;
                if (damageEffecter != null)
                {
                    if (pawn.health.woundedEffecter != null && pawn.health.woundedEffecter.def != damageEffecter)
                    {
                        pawn.health.woundedEffecter.Cleanup();
                    }
                    pawn.health.woundedEffecter = damageEffecter.Spawn();
                    pawn.health.woundedEffecter.Trigger(pawn, launcher ?? pawn);
                    pawn.health.woundedEffecter.Trigger(pawn, launcher ?? pawn);
                    pawn.health.woundedEffecter.Trigger(pawn, launcher ?? pawn);
                }
            }
            if (hitThing is Plant plant)
            {
                if ((plant.def.ingestible != null && plant.def.ingestible.foodType == FoodTypeFlags.Tree) || plant.def.defName == "BurnedTree")
                {
                    removeValue = TreeEquivalent;
                    Effecter effecter = EffecterDefOf.Effecter_3HST_RGhitTreeTrunk.Spawn();
                    TargetInfo targetInfo = new TargetInfo(plant.Position, Map, false);
                    effecter.Trigger(targetInfo, targetInfo);
                    effecter.Cleanup();
                    if (!plant.LeaflessNow && plant.def.defName != "Plant_SaguaroCactus")
                    {
                        Effecter effecter2 = EffecterDefOf.Effecter_3HST_RGhitTreeLeafSpring.Spawn();
                        effecter2.Trigger(targetInfo, targetInfo);
                        effecter2.Cleanup();
                    }
                }
            }
            if (hitThing != null && IsChunk(hitThing.def.thingCategories))
            {
                removeValue = ChunkEquivalent;
            }
            if (hitThing != null)
            {
                attackedThings.Add(hitThing);
            }
            if (penetratingPower - PenetratingPowerBase <= removeValue)
            {
                ShouldTerminate = true;
            }
            base.Impact(hitThing);
            if (def.projectile.explosionRadius > 0)
            {
                Explode();
            }
            canDestroyNow = true;
            landed = false;
            if (hitThing == null)
            {
                if (Rand.Chance(ArmorPenetration / def.projectile.GetArmorPenetration(weaponDamageMultiplier)))
                {
                    removeValue = BounceEquivalent;
                }
                else
                {
                    penetratingPower = 0;
                }
            }
            penetratingPower -= removeValue;
            fuseActivated = true;
            Retarget();
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!canDestroyNow)
            {
                return;
            }
            base.Destroy(mode);
        }

        public void Retarget()
        {
            float remainingRange = 55;
            if (equipmentDef != null && equipmentDef.Verbs.Any())
            {
                remainingRange = equipmentDef.Verbs[0].range - (ExactPosition - trueOrigin).magnitude + ExtraRange;
            }
            if (remainingRange <= 0)
            {
                canDestroyNow = true;
                Destroy();
                return;
            }
            if (remainingRange > MaxSearchRadius) remainingRange = MaxSearchRadius;
            int num = GenRadial.NumCellsInRadius(remainingRange);
            int tries = 20 + (int)remainingRange;
            IntVec3 retargetCell = IntVec3.Invalid;
            List<Thing> affectedTargets = new List<Thing>();

            //20 times of trying random
            while (tries > 0)
            {
                int i = Rand.Range(1, num);
                if (Math.Abs(Vector3.Angle(initialVector, GenRadial.RadialPattern[i].ToVector3().Yto0())) < PostPenetrationDeviationAngle)
                {
                    retargetCell = Position + GenRadial.RadialPattern[i];
                    break;
                }
                tries--;
            }

            //Fall back enum check
            if (!retargetCell.IsValid)
            {
                List<IntVec3> affectedCells = new List<IntVec3>();
                for (int i = 1; i < num; i++)
                {
                    if (Math.Abs(Vector3.Angle(initialVector, GenRadial.RadialPattern[i].ToVector3().Yto0())) < PostPenetrationDeviationAngle)
                    {
                        affectedCells.Add(Position + GenRadial.RadialPattern[i]);
                    }
                }
                if (affectedCells.Any())
                {
                    if (DebugSettings.godMode)
                    {
                        foreach (var c in affectedCells)
                        {
                            Map.debugDrawer.FlashCell(c);
                        }
                    }
                    retargetCell = affectedCells[Rand.Range(0, affectedCells.Count - 1)];
                }
            }
            if (!retargetCell.IsValid)
            {
                canDestroyNow = true;
                Destroy();
                return;
            }
            else if (Map != null && Position.InBounds(Map))
            {
                origin = ExactPosition;
                if (retargetCell.InBounds(Map)) affectedTargets = retargetCell.GetThingList(Map).ToList();
                Thing target = null;
                if (DebugSettings.godMode) Map.debugDrawer.FlashCell(retargetCell);
                if (affectedTargets.Count > 0)
                {
                    target = affectedTargets[Rand.Range(0, affectedTargets.Count - 1)];
                }
                usedTarget = target == null ? retargetCell : new LocalTargetInfo(target);
                destination = usedTarget.Cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.3f);
                ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
                if (ticksToImpact < 1)
                {
                    ticksToImpact = 1;
                }
            }
        }
        private static float GetPawnArmor(Pawn pawn)
        {
            float statValue = pawn.GetStatValue(RimWorld.StatDefOf.ArmorRating_Sharp, true);
            float num = statValue;
            List<Apparel> list = pawn.apparel?.WornApparel ?? Enumerable.Empty<Apparel>().ToList();
            foreach (Apparel apparel2 in list)
            {
                num += apparel2.GetStatValue(RimWorld.StatDefOf.ArmorRating_Sharp, true) * apparel2.def.apparel.HumanBodyCoverage;
            }
            if (num > 0.0001f)
            {
                foreach (BodyPartRecord bodyPartRecord in pawn.RaceProps.body.AllParts)
                {
                    BodyPartRecord bodyPartRecord2 = bodyPartRecord;
                    float num2 = statValue;
                    if (bodyPartRecord2.depth == BodyPartDepth.Outside && (bodyPartRecord2.coverage >= 0.1 || bodyPartRecord2.def == BodyPartDefOf.Eye || bodyPartRecord2.def == BodyPartDefOf.Torso))
                    {
                        foreach (Apparel apparel3 in list)
                        {
                            if (apparel3.def.apparel.CoversBodyPart(bodyPartRecord2))
                            {
                                num2 += apparel3.GetStatValue(RimWorld.StatDefOf.ArmorRating_Sharp, true);
                            }
                        }
                        return num2;
                    }
                }
            }
            return num;
        }
        public static bool IsChunk(List<ThingCategoryDef> thingCategory)
        {
            foreach (ThingCategoryDef x in thingCategory ?? Enumerable.Empty<ThingCategoryDef>())
            {
                if (IsChunk(x)) return true;
            }
            return false;
        }
        public static bool IsChunk(ThingCategoryDef thingCategory)
        {
            if (thingCategory == ThingCategoryDefOf.Chunks || ThingCategoryDefOf.Chunks.childCategories.Contains(thingCategory)) return true;
            return false;
        }

        protected virtual void Explode()
        {
            Map map = base.Map;
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(base.Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, affectedAngle: null, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref attackedThings, "attackedThings", LookMode.Reference);
            Scribe_Values.Look(ref penetratingPower, "penetratingPower");
        }
        public static bool canDestroyNow = true;
        public List<Thing> attackedThings = new List<Thing>();
        public float penetratingPower;
    }

    public class ModExtension_PenetratingProjectile : DefModExtension
    {
        public float overpenDamageMulti = 0.5f;

        public float stopperDamageMulti = 1f;

        public float PenetrationFloorPercentage = 0.2f;

        public float penetrationPotentialMultiplier = 1;

        public float postPenetrationDeviationAngle = 15;

        public float minSearchRadius = 5;

        public float maxSearchRadius = 20;

        public float extraRange = 20;

        public float buildingEquivalentMulti = 0.005f;

        public float armorEquivalentMulti = 1f;

        public float bodysizeEquivalentMulti = 0.1f;

        public float treeEquivalent = 0.1f;

        public float chunkEquivalent = 0.25f;

        public float bounceEquivalent = 0.2f;
    }
}
