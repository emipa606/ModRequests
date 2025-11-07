using RimWorld;
using Vehicles;

namespace BillDoorsFramework
{
    [DefOf]
    public static class VFStatDefOf
    {
        static VFStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VehicleStatDefOf));
        }
        public static VehicleStatDef BDsVehiclePowerRecharge;

        public static VehicleStatDef BDsVehiclePowerCapacitor;

        public static VehicleStatDef BDsVehicleGeneratorFuelUseage;
    }
}
