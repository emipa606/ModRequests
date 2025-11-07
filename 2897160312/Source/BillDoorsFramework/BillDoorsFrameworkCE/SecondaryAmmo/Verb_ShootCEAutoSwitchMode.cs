using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    class Verb_ShootCEAutoSwitchMode : Verb_ShootCE
    {

        public CompSecondaryAmmo SecondaryAmmo
        {
            get
            {
                if (secondaryAmmo != null)
                {
                    return secondaryAmmo;
                }
                secondaryAmmo = this.EquipmentSource.TryGetComp<CompSecondaryAmmo>();
                return secondaryAmmo;
            }
        }
        public override bool TryCastShot()
        {
            //Log.Message("[3HST]Can hit directly: " + this.CanHitCellFromCellIgnoringRange(this.caster.Position, this.currentTarget.Cell, false).ToString());
            bool canHitDirectly = this.CanHitCellFromCellIgnoringRange(this.caster.Position, this.currentTarget.Cell, false);
            if (caster != null && caster.Faction != Faction.OfPlayer)
            {
                this.UseSecondaryAmmo(!canHitDirectly);
            }

            /*
            if (SecondaryAmmo != null)
            {
                Log.Message("[3HST]Secondaty ammo selected: " + SecondaryAmmo.IsSecondaryAmmoSelected);
            }
            else
            {
                Log.Message("[3HST]Cannot find CompSecondaryAmmo");
            }
            */
            if (caster.Faction == Faction.OfPlayer && !SecondaryAmmo.IsSecondaryAmmoSelected && !canHitDirectly)
            {
                return false;
            }

            return base.TryCastShot();
        }

        private bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
        {
            if (this.verbProps.mustCastOnOpenGround && (!targetLoc.Standable(this.caster.Map) || this.caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
            {
                return false;
            }
            if (!this.verbProps.requireLineOfSight)
            {
                if (!includeCorners)
                {
                    if (!GenSight.LineOfSight(sourceSq, targetLoc, this.caster.Map, true, null, 0, 0))
                    {
                        return false;
                    }
                }
                else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, this.caster.Map, true, null))
                {
                    return false;
                }
            }
            return true;
        }

        private void UseSecondaryAmmo(bool use)
        {
            if (SecondaryAmmo == null)
            {
                return;
            }

            if (SecondaryAmmo.IsSecondaryAmmoSelected ^ use)
            {
                SecondaryAmmo.SwitchLauncher();
            }
        }

        CompSecondaryAmmo secondaryAmmo;
    }

    class Verb_ShootCEAutoSwitchModeOneUse : Verb_ShootCEOneUse
    {

        public CompSecondaryVerbCE SecondaryAmmo
        {
            get
            {
                if (secondaryAmmo != null)
                {
                    return secondaryAmmo;
                }
                secondaryAmmo = this.EquipmentSource.TryGetComp<CompSecondaryVerbCE>();
                return secondaryAmmo;
            }
        }
        public override bool TryCastShot()
        {
            //Log.Message("[3HST]Can hit directly: " + this.CanHitCellFromCellIgnoringRange(this.caster.Position, this.currentTarget.Cell, false).ToString());

            bool canHitDirectly = this.CanHitCellFromCellIgnoringRange(this.caster.Position, this.currentTarget.Cell, false);
            if (caster != null && caster.Faction != Faction.OfPlayer)
            {
                this.UseSecondaryAmmo(!canHitDirectly);
            }

            if (caster.Faction == Faction.OfPlayer && !SecondaryAmmo.IsSecondaryVerbSelected && !canHitDirectly)
            {
                return false;
            }

            //Log.Message((this.CompCharges == null).ToString());
            return base.TryCastShot();
        }

        private bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
        {
            if (this.verbProps.mustCastOnOpenGround && (!targetLoc.Standable(this.caster.Map) || this.caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
            {
                return false;
            }
            if (!this.verbProps.requireLineOfSight)
            {
                if (!includeCorners)
                {
                    if (!GenSight.LineOfSight(sourceSq, targetLoc, this.caster.Map, true, null, 0, 0))
                    {
                        return false;
                    }
                }
                else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, this.caster.Map, true, null))
                {
                    return false;
                }
            }
            return true;
        }

        private void UseSecondaryAmmo(bool use)
        {
            if (SecondaryAmmo == null)
            {
                return;
            }

            if (SecondaryAmmo.IsSecondaryVerbSelected ^ use)
            {
                SecondaryAmmo.SwitchVerb();
            }
        }

        CompSecondaryVerbCE secondaryAmmo;
    }
}
