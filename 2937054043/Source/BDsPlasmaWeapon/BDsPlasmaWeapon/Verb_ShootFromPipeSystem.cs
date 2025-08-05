using Verse;
using CombatExtended;
using RimWorld;
using PipeSystem;
using BillDoorsFramework;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootFromPipeSystem : Verb_ShootCE
    {

        private CompProperties_ExplosiveCECone DataConeExplosion => Projectile.CompDefFor<CompExplosiveCECone>() as CompProperties_ExplosiveCECone;


        public CompShootFromPipeNet compShootFromPipeNet
        {
            get
            {
                if (caster is Building_TurretGunCE turret)
                {
                    return turret.TryGetComp<CompShootFromPipeNet>();
                }
                return null;
            }
        }

        public override bool TryCastShot()
        {

            if (base.TryCastShot())
            {
                if (compShootFromPipeNet == null)
                {
                    Log.Error("You forgot to add CompShootFromPipeNet to a Verb_ShootFromPipeSystem using turret");
                    return false;
                }
                else
                {
                    PipeNet pipeNet = compShootFromPipeNet.PipeNet;
                    if (pipeNet != null && pipeNet.Stored >= compShootFromPipeNet.Props.resourceConsumption)
                    {
                        pipeNet.DrawAmongStorage(compShootFromPipeNet.Props.resourceConsumption, pipeNet.storages);
                        return true;
                    }
                    return false;
                }
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
                    RenderPredictedAreaOfEffect(target.Cell, DataConeExplosion.explosiveRadius, DataConeExplosion.angle, direction);
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

                if (radius > 54)
                {
                    radius = 54;
                }
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

        public override bool Available()
        {

            if (base.Available())
            {
                if (compShootFromPipeNet == null)
                {
                    Log.Error("You forgot to add CompShootFromPipeNet to a Verb_ShootFromPipeSystem using turret");
                    return false;
                }
                else
                {
                    PipeNet pipeNet = compShootFromPipeNet.PipeNet;
                    return (pipeNet != null && pipeNet.Stored >= compShootFromPipeNet.Props.resourceConsumption);
                }
            }
            return false;
        }
    }

    public class Verb_ShootNotUnderRoofPipedCE : Verb_ShootFromPipeSystem
    {
        ModExtension_VerbNotUnderRoof extension => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();

        CompSecondaryVerbCE compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerbCE>();

        CompSecondaryAmmo compSecondaryAmmo => EquipmentSource.TryGetComp<CompSecondaryAmmo>();
        public override bool Available()
        {
            if (Caster.Position.Roofed(Caster.Map))
            {
                if (extension == null || (compSecondaryVerb != null && ((compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInPrimaryMode))) || (compSecondaryAmmo != null && ((compSecondaryAmmo.IsSecondaryAmmoSelected && extension.appliesInSecondaryMode) || (!compSecondaryAmmo.IsSecondaryAmmoSelected && extension.appliesInPrimaryMode))))
                {
                    return false;
                }
            }
            return base.Available();
        }
    }
}
