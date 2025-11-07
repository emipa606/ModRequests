using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using static HarmonyLib.Code;

namespace BillDoorsFramework
{
    public class PenetratingBulletCE : BulletCE
    {
        ModExtension_PenetratingProjectile Extension => def.GetModExtension<ModExtension_PenetratingProjectile>();

        public float PenetratingPowerBase => ((ProjectilePropertiesCE)this.def.projectile).armorPenetrationSharp * PenetrationFloorPercentage;
        public override float PenetrationAmount => (Extension == null) ? penetratingPower : penetratingPower / Extension.penetrationPotentialMultiplier;

        public float overpenDamageMulti => (Extension == null) ? 0.5f : Extension.overpenDamageMulti;

        public float PenetrationFloorPercentage => (Extension == null) ? 0.2f : Extension.PenetrationFloorPercentage;

        public bool ShouldTerminate;
        public override float DamageAmount => ShouldTerminate ? def.projectile.GetDamageAmount(StopperDamageMulti) : def.projectile.GetDamageAmount(overpenDamageMulti);
        public float PostPenetrationDeviationAngle => (Extension == null) ? 0 : Extension.postPenetrationDeviationAngle;
        public float BuildingEquivalentMulti => (Extension == null) ? 0.125f : Extension.buildingEquivalentMulti;
        public float BodysizeEquivalentMulti => (Extension == null) ? 5f : Extension.bodysizeEquivalentMulti;
        public float TreeEquivalent => (Extension == null) ? 5f : Extension.treeEquivalent;
        public float ChunkEquivalent => (Extension == null) ? 25f : Extension.chunkEquivalent;
        public float BounceEquivalent => (Extension == null) ? 10f : Extension.bounceEquivalent;
        public float StopperDamageMulti => (Extension == null) ? 1f : Extension.stopperDamageMulti;
        public float ArmorEquivalentMulti => (Extension == null) ? 1f : Extension.armorEquivalentMulti;

        public float MaxPenetratingPower = 0;

        public float ticksToDetonation = 100;

        public bool fuseActivated = false;


        public CompPostPenetrationExplosive compExplosive => this.TryGetComp<CompPostPenetrationExplosive>();

        IntVec3 cachedPosition;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            penetratingPower = ((ProjectilePropertiesCE)this.def.projectile).armorPenetrationSharp;
            if (Extension != null)
            {
                penetratingPower *= Extension.penetrationPotentialMultiplier;
            }
            MaxPenetratingPower = penetratingPower;
            if (compExplosive != null)
            {
                ticksToDetonation = compExplosive.Props.wickTicks.RandomInRange;
            }
        }
        public override void Tick()
        {
            if (penetratingPower <= PenetratingPowerBase)
            {
                Destroy();
                return;
            }
            if (landed == true)
            {
                landed = false;
            }
            base.Tick();
            if (compExplosive != null && fuseActivated)
            {
                ticksToDetonation--;
                if (ticksToDetonation < 0 || penetratingPower <= PenetratingPowerBase)
                {
                    this.TryGetComp<CompFragments>()?.Throw(this.ExactPosition, Map, this);
                    compExplosive.Explode(this, this.Map, this.equipmentDef, this.intendedTarget.Thing);
                }
            }
        }
        public override void Impact(Thing hitThing)
        {
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
                removeValue = GetPawnArmor(pawn) * 2f * ArmorEquivalentMulti + (float)Math.Sqrt(pawn.BodySize) * BodysizeEquivalentMulti;
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
            canDestroyNow = true;
            penetratingPower -= removeValue;
            landed = false;
            if (hitThing == null)
            {
                if (Rand.Chance(penetratingPower / MaxPenetratingPower))
                {
                    penetratingPower -= BounceEquivalent;
                }
                else
                {
                    Destroy();
                    return;
                }
            }
            this.shotSpeed = this.def.projectile.speed * ((penetratingPower / MaxPenetratingPower) / 2 + 0.5f);
            fuseActivated = true;
            Retarget();
        }

        public void Retarget()
        {
            float angle = 0;
            if (PostPenetrationDeviationAngle > 0)
            {
                angle = Rand.Range(-PostPenetrationDeviationAngle, PostPenetrationDeviationAngle);
            }
            this.shotRotation += angle;
            Launch(launcher, new UnityEngine.Vector2(ExactPosition.x, ExactPosition.z), shotAngle, shotRotation, shotHeight, shotSpeed, equipment, Rand.Range(5, 55));
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!canDestroyNow)
            {
                return;
            }
            base.Destroy(mode);
        }
        private static float GetPawnArmor(Pawn pawn)
        {
            float statValue = pawn.GetStatValue(StatDefOf.ArmorRating_Sharp, true);
            float num = statValue;
            List<Apparel> list = pawn.apparel?.WornApparel ?? Enumerable.Empty<Apparel>().ToList();
            foreach (Apparel apparel2 in list)
            {
                num += apparel2.GetStatValue(StatDefOf.ArmorRating_Sharp, true) * apparel2.def.apparel.HumanBodyCoverage;
            }
            if (num > 0.0001f)
            {
                foreach (BodyPartRecord bodyPartRecord in pawn.RaceProps.body.AllParts)
                {
                    BodyPartRecord bodyPartRecord2 = bodyPartRecord;
                    float num2 = bodyPartRecord.IsInGroup(CE_BodyPartGroupDefOf.CoveredByNaturalArmor) ? statValue : 0f;
                    if (bodyPartRecord2.depth == BodyPartDepth.Outside && ((double)bodyPartRecord2.coverage >= 0.1 || bodyPartRecord2.def == BodyPartDefOf.Eye || bodyPartRecord2.def == BodyPartDefOf.Torso))
                    {
                        foreach (Apparel apparel3 in list)
                        {
                            if (apparel3.def.apparel.CoversBodyPart(bodyPartRecord2))
                            {
                                num2 += apparel3.GetStatValue(StatDefOf.ArmorRating_Sharp, true);
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
}
