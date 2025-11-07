using System.Linq;
using Vehicles;
using Verse;

namespace BillDoorsFramework
{
    internal class VehicleTurret_PowerDrain : VehicleTurret
    {
        ModExtension_TurretPowerDrain ext => this.turretDef.GetModExtension<ModExtension_TurretPowerDrain>();
        CompVehicleCapacitor capacitor => vehicle.TryGetComp<CompVehicleCapacitor>();

        public VehicleTurret_PowerDrain()
        {
        }

        public VehicleTurret_PowerDrain(VehiclePawn vehicle)
        {
            this.vehicle = vehicle;
            vehicleDef = vehicle.VehicleDef;
        }

        public VehicleTurret_PowerDrain(VehiclePawn vehicle, VehicleTurret reference)
        {
            this.vehicle = vehicle;
            vehicleDef = vehicle.VehicleDef;

            uniqueID = Find.UniqueIDsManager.GetNextThingID();
            turretDef = reference.turretDef;

            gizmoLabel = reference.gizmoLabel;

            key = reference.key;

            targetPersists = reference.turretDef.turretType == TurretType.Static ? false : reference.targetPersists;
            autoTargeting = reference.turretDef.turretType == TurretType.Static ? false : reference.autoTargeting;
            manualTargeting = reference.turretDef.turretType == TurretType.Static ? false : reference.manualTargeting;

            currentFireMode = 0;
            currentFireIcon = base.OverheatIcons.FirstOrDefault();
            ticksSinceLastShot = 0;
            burstsTillWarmup = 0;

            ResolveCannonGraphics(vehicle);
            InitRecoilTrackers();
        }

        public override bool TurretEnabled(VehicleDef vehicleDef, TurretDisableType turretKey)
        {
            return (capacitor == null || ext == null || capacitor.Power >= ext.powerPerShot) && base.TurretEnabled(vehicleDef, turretKey);
        }


        public override void FireTurret()
        {
            if (capacitor == null || ext == null || capacitor.Power >= ext.powerPerShot)
            {
                base.FireTurret();
            }
        }

        public override void PostTurretFire()
        {
            base.PostTurretFire();
            if (capacitor != null && ext != null)
            {
                capacitor.TryDrain(ext.powerPerShot);
            }
        }
    }

    internal class ModExtension_TurretPowerDrain : DefModExtension
    {
        public float powerPerShot;
    }
}
