using Verse;
using RimWorld;
using Verse.AI;
using System;
using BillDoorsFramework;
using System.Collections.Generic;
using UnityEngine;

namespace BDsPlasmaWeaponVanilla
{
    public class Verb_ShootFromBackpackTank : Verb_Shoot
    {

        public CompTankFeedWeapon compTankFeedWeapon => EquipmentSource.TryGetComp<CompTankFeedWeapon>();

        private CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();

        private ModExtension_Verb_Shotgun DataShotgun => EquipmentSource.def.GetModExtension<ModExtension_Verb_Shotgun>();

        private bool isOvercharged => compSecondaryVerb != null && compSecondaryVerb.IsSecondaryVerbSelected;

        public CompReloadableFromFiller compTank
        {
            get
            {
                CompReloadableFromFiller comp = EquipmentSource.TryGetComp<CompReloadableFromFiller>();
                if (comp != null && (compTankFeedWeapon != null && comp.RemainingCharges >= compTankFeedWeapon.Props.ammoConsumption))
                {
                    return comp;
                }
                else if (compTankFeedWeapon != null)
                {
                    return compTankFeedWeapon.compReloadableFromFiller;
                }
                return null;
            }
        }

        private int ammoConsumption => isOvercharged ? compTankFeedWeapon.Props.ammoConsumption * 2 : compTankFeedWeapon.Props.ammoConsumption;

        public override bool Available()
        {
            if (base.Available())
            {
                if (compTank == null || compTankFeedWeapon == null)
                {
                    return false;
                }
                if (compTank.RemainingCharges < ammoConsumption)
                {
                    return compTankFeedWeapon.searchTank(ammoConsumption, false);
                }
                return true;
            }
            return false;
        }

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (!(CasterIsPawn && CasterPawn.Faction != Faction.OfPlayer) && (compTank == null || compTank.RemainingCharges < ammoConsumption))
                {
                    compTankFeedWeapon?.searchTank();
                    return false;
                }
                if (isOvercharged && BDPMod.OverchargeDamageWeapon)
                {
                    if (Caster is Building turret)
                    {
                        compTankFeedWeapon.OverchargedDamage(turret);
                    }
                    else
                    {
                        compTankFeedWeapon.OverchargedDamage(EquipmentSource);
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
                compTank.DrawGas(ammoConsumption, false);
                if (DataShotgun != null && DataShotgun.ShotgunPellets > 1)
                {
                    for (int i = 1; i < DataShotgun.ShotgunPellets; i++)
                    {
                        base.TryCastShot();
                    }
                }
                return true;
            }
            return false;
        }
    }


    public class Verb_ShootOverchargeDamage : Verb_Shoot
    {

        public DefModExtension_VerbOverchargeDamage Data
        {
            get
            {
                return EquipmentSource.def.GetModExtension<DefModExtension_VerbOverchargeDamage>();
            }
        }

        private CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();


        private DefModExtension_ConeExplosion DataConeExplosion => Projectile.GetModExtension<DefModExtension_ConeExplosion>();


        private bool isOvercharged => compSecondaryVerb != null && compSecondaryVerb.IsSecondaryVerbSelected;

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (Data != null && isOvercharged && BDPMod.OverchargeDamageWeapon)
                {
                    if (Caster is Building turret)
                    {
                        OverchargedDamage(turret);
                    }
                    else
                    {
                        OverchargedDamage(EquipmentSource);
                    }
                }
                return true;
            }
            return false;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            verbProps.DrawRadiusRing(caster.Position);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                if (DataConeExplosion == null)
                {
                    DrawHighlightFieldRadiusAroundTarget(target);
                    return;
                }
                else
                {
                    Vector3 direction = (caster.Position - target.Cell).ToVector3();
                    RenderPredictedAreaOfEffect(target.Cell, Projectile.projectile.explosionRadius, DataConeExplosion.angle, direction);
                }
            }
        }


        IntVec3 cachedLoc;
        List<IntVec3> cachedAffectedCells = new List<IntVec3>();



        public void RenderPredictedAreaOfEffect(IntVec3 loc, float radius, float angle, Vector3 direction)
        {
            if (cachedLoc == loc)
            {
                GenDraw.DrawFieldEdges(cachedAffectedCells);
            }
            else
            {
                cachedLoc = loc;

                int num = GenRadial.NumCellsInRadius(radius);
                List<IntVec3> affectedCells = new List<IntVec3>();
                for (int i = 1; i < num; i++)
                {
                    IntVec3 intVec = loc + GenRadial.RadialPattern[i];
                    Vector3 vec3 = (loc - intVec).ToVector3();
                    vec3.y = 0;
                    if (Math.Abs(Vector3.Angle(direction, vec3)) < angle)
                    {
                        affectedCells.Add(intVec);
                    }
                }

                cachedAffectedCells = affectedCells;
                GenDraw.DrawFieldEdges(affectedCells);
            }
        }

        public void OverchargedDamage(ThingWithComps weapon)
        {
            if (Rand.Chance(Data.overchargeDamageChance))
            {
                float HPcache = (float)weapon.HitPoints / weapon.MaxHitPoints;
                weapon.HitPoints -= (int)Math.Round(Rand.Value * Data.overchargeDamageMultiplier);
                float HPnow = (float)weapon.HitPoints / weapon.MaxHitPoints;

                if (EquipmentSource.ParentHolder is Thing thing && thing.Faction != Faction.OfPlayer)
                {
                    return;
                }

                if (EquipmentSource.ParentHolder is Pawn pawn)
                {
                    if (HPcache > 0.5 && HPnow <= 0.5)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingPawn".Translate(), pawn, EquipmentSource.LabelCap), (Thing)EquipmentSource.ParentHolder, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (HPcache > 0.25 && HPnow <= 0.25)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingUrgentPawn".Translate(), pawn, EquipmentSource.LabelCap), (Thing)EquipmentSource.ParentHolder, MessageTypeDefOf.ThreatSmall, historical: false);
                    }
                }
                else
                {
                    if (HPcache > 0.5 && HPnow <= 0.5)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailing".Translate(), EquipmentSource.LabelCap), (Thing)EquipmentSource.ParentHolder, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (HPcache > 0.25 && HPnow <= 0.25)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingUrgent".Translate(), EquipmentSource.LabelCap), (Thing)EquipmentSource.ParentHolder, MessageTypeDefOf.ThreatSmall, historical: false);
                    }
                }
            }
        }
    }

    public class DefModExtension_VerbOverchargeDamage : DefModExtension
    {
        public float overchargeDamageChance = 0;
        public float overchargeDamageMultiplier = 1;
    }
}
