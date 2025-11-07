using System.Linq;
using Vehicles;
using Verse;
using Verse.Noise;

namespace BillDoorsFramework
{
    internal class VehicleTurret_Custom : VehicleTurret
    {
        public string turretComponentKey;

        public VehicleTurret_Custom()
        {
        }

        public VehicleTurret_Custom(VehiclePawn vehicle)
        {
            this.vehicle = vehicle;
            vehicleDef = vehicle.VehicleDef;
        }

        public VehicleTurret_Custom(VehiclePawn vehicle, VehicleTurret_Custom reference)
        {
            this.vehicle = vehicle;
            vehicleDef = vehicle.VehicleDef;

            uniqueID = Find.UniqueIDsManager.GetNextThingID();
            turretDef = reference.turretDef;

            gizmoLabel = reference.gizmoLabel;

            key = reference.key;
            turretComponentKey = reference.turretComponentKey;

            targetPersists = reference.turretDef.turretType != TurretType.Static && reference.targetPersists;
            autoTargeting = reference.turretDef.turretType != TurretType.Static && reference.autoTargeting;
            manualTargeting = reference.turretDef.turretType != TurretType.Static && reference.manualTargeting;

            currentFireMode = 0;
            currentFireIcon = base.OverheatIcons.FirstOrDefault();
            ticksSinceLastShot = 0;
            burstsTillWarmup = 0;

            ResolveCannonGraphics(vehicle);
            InitRecoilTrackers();
        }
    }

    internal class ModExtension_TurretCustom : DefModExtension
    {
        public CurveSimple turnSpeedCurve;

        public CurveSimple rotateSpeedCurve;
    }
}
