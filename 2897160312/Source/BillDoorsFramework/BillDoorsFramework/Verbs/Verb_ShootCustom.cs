using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class Verb_ShootCustom : Verb_Shoot
    {
        public DefModExtension_ShootUsingRandomProjectileBase DataRandProj
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingRandomProjectileBase>();
            }
        }
        ModExtension_Verb_Shotgun DataShotgun => EquipmentSource.def.GetModExtension<ModExtension_Verb_Shotgun>();
        ModExtension_VerbNotUnderRoof DataNotUnderRoof => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();
        DefModExtension_ShootUsingMechBattery DataMechBattery => EquipmentSource.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
        ModExtension_RandomBurstBreak randomBurstBreak => EquipmentSource.def.GetModExtension<ModExtension_RandomBurstBreak>();
        ModExtension_DropItemWhenFire DataDropItem => EquipmentSource.def.GetModExtension<ModExtension_DropItemWhenFire>();
        ModExtension_OneUse DataOneUse => EquipmentSource.def.GetModExtension<ModExtension_OneUse>();
        ModExtension_ProjOriginOffset DataOriginOffset => EquipmentSource.def.GetModExtension<ModExtension_ProjOriginOffset>();

        ModExtension_MultiShot DataMultiShot => EquipmentSource.def.GetModExtension<ModExtension_MultiShot>();
        CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();


        int currenBurstRandomIndex;

        public override void WarmupComplete()
        {
            RandomizeProjectile();
            base.WarmupComplete();
            RandomizeBurstCount();
        }

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
            return AvailableNotUnderRoof() && AvailableMechBattery() && base.Available();
        }

        public override void Notify_EquipmentLost()
        {
            base.Notify_EquipmentLost();
            if (state == VerbState.Bursting && burstShotsLeft < verbProps.burstShotCount)
            {
                TryCastShotOneUse();
            }
        }

        protected override bool TryCastShot()
        {
            if (DataRandProj != null && DataRandProj.randomWithinBurst)
            {
                RandomizeProjectile();
            }
            var casterdp = Caster.DrawPos;
            if (base.TryCastShot())
            {
                TryCastShotMultiShot();
                TryCastShotMechBattery();
                TryCastShotShotgun();
                TryCastShotRandomBurstBreak();
                TryCastShotDropItemWhenFire();
                TryCastShotOneUse();
                return true;
            }
            return false;
        }

        public bool AvailableNotUnderRoof()
        {
            return !(DataNotUnderRoof != null && Caster.Position.Roofed(Caster.Map) && (compSecondaryVerb == null || (compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInPrimaryMode)));
        }

        public bool AvailableMechBattery()
        {
            Need_MechEnergy battery = CasterPawn?.needs.TryGetNeed<Need_MechEnergy>();
            if (battery != null)
            {
                return battery.CurLevel > DataMechBattery.energyConsumption;
            }
            return true;
        }

        public void RandomizeProjectile()
        {
            if (DataRandProj != null)
            {
                verbProps.defaultProjectile = DataRandProj.GetProjectile();
            }
        }

        public void RandomizeBurstCount()
        {
            if (randomBurstBreak != null)
            {
                burstShotsLeft += randomBurstBreak.randomBurst.RandomInRange;
            }
        }

        void TryCastShotMechBattery()
        {
            if (ModLister.BiotechInstalled && DataMechBattery != null)
            {
                Need_MechEnergy battery = CasterPawn?.needs.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    battery.CurLevel -= DataMechBattery.energyConsumption;
                }
            }
        }

        void TryCastShotShotgun()
        {
            if (DataShotgun != null)
            {
                if (DataShotgun.ShotgunPellets > 1)
                {
                    for (int i = 1; i < DataShotgun.ShotgunPellets; i++)
                    {
                        base.TryCastShot();
                    }
                }
                if (DataShotgun.extraProjectile != null && DataShotgun.extraProjectileCount > 0)
                {
                    ThingDef originalProjectile = verbProps.defaultProjectile;
                    verbProps.defaultProjectile = DataShotgun.extraProjectile;
                    for (int i = 0; i < DataShotgun.extraProjectileCount; i++)
                    {
                        base.TryCastShot();
                    }
                    verbProps.defaultProjectile = originalProjectile;
                }
            }
        }

        void TryCastShotMultiShot()
        {
            if (DataMultiShot != null && DataMultiShot.shotCount > 1)
            {
                float magnitude = Projectile.projectile.SpeedTilesPerTick / DataMultiShot.shotCount;
                for (int i = 1; i < DataMultiShot.shotCount; i++)
                {
                    //Honestly I'd rather not having to do this

                    ShootLine resultingLine;
                    TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
                    if (base.EquipmentSource != null)
                    {
                        base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                        base.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
                        base.EquipmentSource.GetComp<CompEquippableAbilityReloadable>()?.UsedOnce();
                    }
                    lastShotTick = Find.TickManager.TicksGame;
                    Thing manningPawn = caster;
                    Thing equipmentSource = base.EquipmentSource;
                    CompMannable compMannable = caster.TryGetComp<CompMannable>();
                    if (compMannable != null && compMannable.ManningPawn != null)
                    {
                        manningPawn = compMannable.ManningPawn;
                        equipmentSource = caster;
                    }
                    Vector3 origin = caster.DrawPos;
                    Projectile projectile2 = (Projectile)GenSpawn.Spawn(Projectile, resultingLine.Source, caster.Map);
                    if (verbProps.ForcedMissRadius > 0.5f)
                    {
                        float num = verbProps.ForcedMissRadius;
                        if (manningPawn != null && manningPawn is Pawn pawn) num *= verbProps.GetForceMissFactorFor(equipmentSource, pawn);
                        float num2 = VerbUtility.CalculateAdjustedForcedMiss(num, currentTarget.Cell - caster.Position);
                        if (num2 > 0.5f)
                        {
                            IntVec3 forcedMissTarget = GetForcedMissTarget(num2);
                            if (forcedMissTarget != currentTarget.Cell)
                            {
                                ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                                if (Rand.Chance(0.5f)) projectileHitFlags = ProjectileHitFlags.All;
                                if (!canHitNonTargetPawnsNow) projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;

                                LaunchProjectileWithOffset(projectile2, magnitude * i, manningPawn, origin, forcedMissTarget, currentTarget, projectileHitFlags, preventFriendlyFire, equipmentSource);
                                return;
                            }
                        }
                    }
                    ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
                    Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                    ThingDef targetCoverDef = randomCoverToMissInto?.def;

                    if (verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
                    {
                        resultingLine.ChangeDestToMissWild_NewTemp(shotReport.AimOnTargetChance_StandardTarget, Projectile.projectile.flyOverhead, caster.Map);
                        ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow) projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;

                        LaunchProjectileWithOffset(projectile2, magnitude * i, manningPawn, origin, resultingLine.Dest, currentTarget, projectileHitFlags2, preventFriendlyFire, equipmentSource, targetCoverDef);
                        return;
                    }

                    if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover && !Rand.Chance(shotReport.PassCoverChance))
                    {
                        ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                        if (canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                        }
                        LaunchProjectileWithOffset(projectile2, magnitude * i, manningPawn, origin, randomCoverToMissInto, currentTarget, projectileHitFlags3, preventFriendlyFire, equipmentSource, targetCoverDef);
                        return;
                    }

                    ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
                    if (canHitNonTargetPawnsNow) projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
                    if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full) projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
                    if (currentTarget.Thing != null)
                    {
                        LaunchProjectileWithOffset(projectile2, magnitude * i, manningPawn, origin, currentTarget, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource);
                    }
                    else
                    {
                        LaunchProjectileWithOffset(projectile2, magnitude * i, manningPawn, origin, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource);
                    }
                }
            }
        }

        protected virtual void LaunchProjectileWithOffset(Projectile Proj, float magnitude, Thing lcr, Vector3 origin, LocalTargetInfo uc, LocalTargetInfo ic, ProjectileHitFlags hitFlags, bool pff = false, Thing eq = null, ThingDef cov = null)
        {
            Vector3 vector = new Vector3();
            if (uc.HasThing)
            {
                vector = uc.Thing.TrueCenter() - origin;
            }
            else
            {
                vector = uc.Cell.ToVector3Shifted() - origin;
            }
            vector.Normalize();
            vector *= magnitude;
            Proj.Launch(lcr, origin + vector, uc, currentTarget, hitFlags, pff, eq, cov);
            if (verbProps.consumeFuelPerShot > 0f)
            {
                caster.TryGetComp<CompRefuelable>()?.ConsumeFuel(verbProps.consumeFuelPerShot);
            }
            burstShotsLeft--;
            TryCastShotMechBattery();
            TryCastShotRandomBurstBreak();
            TryCastShotDropItemWhenFire();
        }

        void TryCastShotRandomBurstBreak()
        {
            if (randomBurstBreak != null && Rand.Chance(randomBurstBreak.chance))
            {
                burstShotsLeft = 1;
            }
        }

        void TryCastShotDropItemWhenFire()
        {
            if (DataDropItem != null)
            {
                Thing thing = ThingMaker.MakeThing(DataDropItem.Thingdef, null);
                if (CasterIsPawn && CasterPawn.Faction.IsPlayer && !DataDropItem.alwaysOnGround)
                {
                    CasterPawn.inventory.innerContainer.TryAdd(thing);
                }
                else
                {
                    thing.SetForbidden(true, false);
                    GenPlace.TryPlaceThing(thing, caster.InteractionCell == null ? caster.Position : caster.InteractionCell, caster.Map, ThingPlaceMode.Near, out _, null, null, default);
                }
            }
        }

        void TryCastShotOneUse()
        {
            if (burstShotsLeft <= 1 && DataOneUse != null)
            {
                if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
                {
                    base.EquipmentSource.Destroy();
                }

                if (CasterIsPawn && !CasterPawn.IsPlayerControlled)
                {
                    Thing thing = GenClosest.ClosestThingReachable(CasterPawn.Position, CasterPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(CasterPawn), 8f, (Thing x) => CasterPawn.CanReserve(x) && !x.IsBurning() && !(x.def.IsRangedWeapon && CasterPawn.WorkTagIsDisabled(WorkTags.Shooting)), null, 0, 15);
                    if (thing != null)
                    {
                        CasterPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RimWorld.JobDefOf.Equip, thing));
                    }
                }
            }
        }
    }
}
