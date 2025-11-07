using CombatExtended;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Verb_DoNothingCE : Verb_ShootCE
    {
        public override bool TryCastShot()
        {
            return true;
        }
    }

    public class Verb_ShootCustomCE : Verb_ShootCE
    {
        #region extensions
        DefModExtension_ShootUsingRandomProjectile DataRandProj => EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingRandomProjectile>();
        ModExtension_VerbNotUnderRoof DataNotUnderRoof => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();
        DefModExtension_ShootUsingMechBattery DataMechBattery => EquipmentSource.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
        ModExtension_RandomBurstBreak randomBurstBreak => EquipmentSource.def.GetModExtension<ModExtension_RandomBurstBreak>();
        ModExtension_DropItemWhenFire DataDropItem => EquipmentSource.def.GetModExtension<ModExtension_DropItemWhenFire>();
        ModExtension_OneUse DataOneUse => EquipmentSource.def.GetModExtension<ModExtension_OneUse>();
        ModExtension_MultiShot DataMultiShot => EquipmentSource.def.GetModExtension<ModExtension_MultiShot>();
        ModExtension_ProjOriginOffset DataOriginOffset => EquipmentSource.def.GetModExtension<ModExtension_ProjOriginOffset>();
        #endregion
        CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();
        CompSecondaryAmmo compSecondaryAmmo => EquipmentSource.TryGetComp<CompSecondaryAmmo>();

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

        public override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                TryCastShotFireAllAtOnce();
                TryCastShotMultiShot();
                TryCastShotOneUse();
                ShotOffset = Vector2.zero;
                return true;
            }
            ShotOffset = Vector2.zero;
            return false;
        }
        #region availabilities
        public bool AvailableNotUnderRoof()
        {
            if (DataNotUnderRoof == null) return true;
            if (Caster != null && Caster.Spawned && Caster.Position.Roofed(Caster.Map))
            {
                if (DataNotUnderRoof == null
                    || (compSecondaryVerb != null && ((compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInPrimaryMode)))
                    || (compSecondaryAmmo != null && ((compSecondaryAmmo.IsSecondaryAmmoSelected && DataNotUnderRoof.appliesInSecondaryMode) || (!compSecondaryAmmo.IsSecondaryAmmoSelected && DataNotUnderRoof.appliesInPrimaryMode))))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AvailableMechBattery()
        {
            if (ModLister.BiotechInstalled && DataMechBattery != null)
            {
                Need_MechEnergy battery = CasterPawn?.needs?.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    return battery.CurLevel > DataMechBattery.energyConsumption;
                }
            }
            return true;
        }
        #endregion
        public void RandomizeProjectile()
        {
            if (DataRandProj != null)
            {
                currenBurstRandomIndex = Rand.Range(0, DataRandProj.projectiles.Count - 1);
                verbProps.defaultProjectile = DataRandProj.projectiles[currenBurstRandomIndex];
            }
        }

        public void RandomizeBurstCount()
        {
            if (randomBurstBreak != null)
            {
                burstShotsLeft += randomBurstBreak.randomBurst.RandomInRange;
            }
        }
        #region tryCastShotFunc
        void TryCastShotMechBattery()
        {
            if (ModLister.BiotechInstalled && DataMechBattery != null)
            {
                Need_MechEnergy battery = CasterPawn?.needs?.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    battery.CurLevel -= DataMechBattery.energyConsumption;
                }
            }
        }

        void TryCastShotRandomBurstBreak()
        {
            if (randomBurstBreak != null && Rand.Chance(randomBurstBreak.chance))
            {
                burstShotsLeft = 1;
            }
        }

        void TryCastShotFireAllAtOnce()
        {
            if (EquipmentSource.def.HasModExtension<ModExtension_FireAllAtOnceCE>())
            {
                int barrelCount = EquipmentSource.def.GetModExtension<ModExtension_FireAllAtOnceCE>().barrelCount;
                for (int i = 1; i < ShotsPerBurst; i++)
                {
                    base.TryCastShot();
                    burstShotsLeft--;
                    if (barrelCount > 1)
                    {
                        MuzzleFlashInvoker.MuzzleFlashInvoker.SpawnMuzzleFlash(this, Rand.Range(0, barrelCount - 1));
                    }
                }
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
                var inventory = ShooterPawn?.TryGetComp<CompInventory>();
                if (!this.EquipmentSource?.Destroyed ?? false)
                {
                    this.EquipmentSource.Destroy(DestroyMode.Vanish);
                }
                if (inventory != null && ShooterPawn?.jobs.curJob?.def != CE_JobDefOf.OpportunisticAttack)
                {
                    var newGun = inventory.rangedWeaponList?.FirstOrDefault(t => t.def == EquipmentSource?.def);
                    if (newGun != null)
                    {
                        inventory.TrySwitchToWeapon(newGun);
                    }
                    else
                    {
                        inventory.SwitchToNextViableWeapon();
                    }
                }
            }
        }

        Vector2 ShotOffset;

        void TryCastShotMultiShot()
        {
            if (DataMultiShot != null && DataMultiShot.shotCount > 1)
            {
                float magnitude = Projectile.projectile.SpeedTilesPerTick / (DataMultiShot.shotCount - 1);
                var LocalSourceOffset = Vector2.up.RotatedBy(shotRotation) * magnitude;
                for (int i = 1; i < DataMultiShot.shotCount; i++)
                {
                    ShotOffset = LocalSourceOffset * i;
                    base.TryCastShot();
                    PostMultiCast();
                }
            }
        }
        #endregion

        public override void ShiftTarget(ShiftVecReport report, Vector3 v, bool calculateMechanicalOnly = false, bool isInstant = false)
        {
            base.ShiftTarget(report, v, calculateMechanicalOnly, isInstant);
            if (!calculateMechanicalOnly)
            {
                if (DataOriginOffset != null)
                {
                    sourceLoc += DataOriginOffset.GetOffsetFor(burstShotsLeft).RotatedBy(shotRotation);
                }
                sourceLoc += ShotOffset;
            }
        }

        protected virtual void PostMultiCast()
        {
            OnCastSuccessful();
            if (verbProps.consumeFuelPerShot > 0f)
            {
                caster.TryGetComp<CompRefuelable>()?.ConsumeFuel(verbProps.consumeFuelPerShot);
            }
            burstShotsLeft--;
        }

        protected override bool OnCastSuccessful()
        {
            if (!base.OnCastSuccessful()) return false;
            TryCastShotMechBattery();
            TryCastShotRandomBurstBreak();
            TryCastShotDropItemWhenFire();
            return true;
        }
        private float GetMinCollisionDistance(float targetDistance)
        {
            var shortRangeMinCollisionDistance = 1.5f;
            var longRangeMinCollisionDistMult = 0.2f;
            if (targetDistance <= shortRangeMinCollisionDistance / longRangeMinCollisionDistMult)
            {
                //For targets at close ranges, skip collisions up to 1.5 cells away (avoids shooter embrasure diagonal collisions),
                //or 75% of the way to the target, whichever is closer.
                return Mathf.Min(shortRangeMinCollisionDistance, targetDistance * 0.75f);
            }
            else
            {
                //At longer ranges, skip collisions on a small % of the flight path,
                //so pawns don't blow themselves up or mag-dump into a wall if weapon sway causes the projectiles to glance hit an obstruction close to the LOS line.
                return targetDistance * longRangeMinCollisionDistMult;
            }
        }
    }
}
