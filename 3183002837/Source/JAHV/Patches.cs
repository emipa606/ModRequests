using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace JAHV
{
    [StaticConstructorOnStartup]
    public static class Patches
    {
        public static FieldInfo shutDown;

        public static MethodInfo getVehicle;

        private static readonly Type patchType;

        static Patches()
        {
            shutDown = AccessTools.Field(typeof(CompProjectileInterceptor), "shutDown");
            getVehicle = AccessTools.PropertyGetter(typeof(ITab_Vehicle_Cargo), "Vehicle");
            patchType = typeof(JAHV.Patches);
            new Harmony("temmie3754.jahv").Patch(AccessTools.Method(typeof(ITab_Vehicle_Cargo), "InterfaceDrop"), new HarmonyMethod(patchType, "Patch_InterfaceDrop"));
        }

        public static bool Patch_InterfaceDrop(ITab_Vehicle_Cargo __instance, Thing thing)
        {
            if (!(thing is VehiclePawn))
            {
                return true;
            }
            VehiclePawn Vehicle = (VehiclePawn)getVehicle.Invoke(__instance, null);
            if (Vehicle.inventory.innerContainer.TryDrop(thing, Vehicle.Position, Vehicle.Map, ThingPlaceMode.Near, out var _, null, (IntVec3 pos) => !Vehicle.Map.thingGrid.ThingsListAt(pos).Any((Thing t) => t is VehiclePawn)))
            {
                Vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
            }
            return false;
        }
    }
}
