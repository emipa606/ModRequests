using RimWorld;
using System;
using System.Linq;
using Vehicles;
using Verse;

namespace BillDoorsFramework
{
    [Obsolete]
    public class TurretRestrictions_TurretRing : TurretRestrictions
    {
        VehicleTurret_Custom Turret_Custom => turret as VehicleTurret_Custom;
        string turretComponentKey => Turret_Custom.turretComponentKey;
        bool deactivated = false;

        public override void Init(VehiclePawn vehicle, VehicleTurret turret)
        {
            base.Init(vehicle, turret);
            if (Turret_Custom == null)
            {
                Log.Warning(turret.key + " of " + vehicle.def.defName + " has TurretRestrictions_TurretRing but is not a turret_Custom");
                deactivated = true;
            }
            else if (turretComponentKey == null)
            {
                Log.Warning(turret.key + " of " + vehicle.def.defName + " has TurretRestrictions_TurretRing but turretComponentKey is empty, ignore if intended");
                deactivated = true;
            }
        }

        public override bool Disabled
        {
            get
            {
                if (deactivated)
                {
                    return false;
                }
                return vehicle.statHandler.components.Any(c => c.props.key == turretComponentKey && c.HealthPercent <= 0);
            }
        }

        public override string DisableReason => "BDFV_TurretStructureDestroyed".Translate();
    }
}
