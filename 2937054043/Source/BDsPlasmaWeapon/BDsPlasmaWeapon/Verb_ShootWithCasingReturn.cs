using Verse;
using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using Verse.Sound;
using System;
using Verse.AI;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootWithCasingReturn : Verb_ShootCE
    {
        private int beepCoolDown = 0;

        public CompCasingReturn CompCasing
        {
            get
            {
                return EquipmentSource.TryGetComp<CompCasingReturn>();
            }
        }

        public CompTankFeedWeapon compTankSwitch
        {
            get
            {
                return EquipmentSource.TryGetComp<CompTankFeedWeapon>();
            }
        }

        public CompReloadableFromFiller compTank;

        public override CompAmmoUser CompAmmo
        {
            get
            {
                if (compTankSwitch != null && compTankSwitch.isOn)
                {
                    compAmmo = null;
                    return null;
                }
                if (compAmmo == null && EquipmentSource != null)
                {
                    compAmmo = EquipmentSource.TryGetComp<CompAmmoUser>();
                }
                return compAmmo;
            }
        }

        public override void VerbTickCE()
        {
            if (beepCoolDown > 0)
            {
                beepCoolDown--;
            }
            base.VerbTickCE();
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            {
                if (Rand.Chance((verbProps.warmupTime + EquipmentSource.GetStatValue(RimWorld.StatDefOf.RangedWeapon_Cooldown)) / 5f) && beepCoolDown < 1)
                {
                    ThingDefOf.BDP_Shot_PlasmaWarmUp.PlayOneShot(caster);
                    beepCoolDown = Rand.Range(300, 1000);
                }
                return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire);
            }
        }


        public override bool Available()
        {
            if (compTankSwitch != null && compTankSwitch.compReloadableFromFiller != null && compTankSwitch.isOn)
            {
                return true;
            }
            return base.Available();
        }

        public override bool TryCastShot()
        {
            if (compTankSwitch != null && compTankSwitch.compReloadableFromFiller != null && compTankSwitch.isOn)
            {
                compTank = compTankSwitch.compReloadableFromFiller;
                if (compTank.remainingCharges < VerbPropsCE.ammoConsumedPerShotCount)
                {
                    compTankSwitch.searchTank(base.VerbPropsCE.ammoConsumedPerShotCount);
                    compTank = compTankSwitch.compReloadableFromFiller;
                }
                if (base.TryCastShot())
                {
                    compTank.DrawGas(VerbPropsCE.ammoConsumedPerShotCount);
                    return true;
                }
            }
            else if (base.TryCastShot())
            {
                if (CompCasing != null && CompAmmo != null)
                {
                    if (BDPMod.OverchargeDamageWeapon && compAmmo.CurrentAmmo == AmmoDefOf.Ammo_LizionCellOvercharged && Caster.Faction == Faction.OfPlayer)
                    {
                        if (Caster is Building turret)
                        {
                            CompCasing.OverchargedDamage(turret);
                        }
                        else
                        {
                            CompCasing.OverchargedDamage(EquipmentSource);
                            if (EquipmentSource.HitPoints <= 0 && CasterPawn != null)
                            {
                                EquipmentSource.Destroy();
                                if (CasterPawn.Spawned)
                                {
                                    if (CasterPawn.stances.curStance is Stance_Warmup)
                                    {
                                        CasterPawn.stances.CancelBusyStanceSoft();
                                    }
                                    if (CasterPawn.CurJob != null)
                                    {
                                        CasterPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                                    }
                                }
                            }
                        }
                    }
                    else if (!BDPMod.OverchargeDontReturnCasing || (compAmmo.CurrentAmmo != AmmoDefOf.Ammo_LizionCellOvercharged))
                    {
                        if (CasterIsPawn && ShooterPawn.Faction == Faction.OfPlayer)
                        {
                            CompCasing.DropCasing(ShooterPawn);
                        }
                        else
                        {
                            CompCasing.DropCasing(Caster.Position, Caster.Map);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
