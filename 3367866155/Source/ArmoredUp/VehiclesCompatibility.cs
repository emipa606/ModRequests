using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ArmoredUp
{
    public static class VehiclesCompatibility
    {
        public static bool VehiclesActive = ModsConfig.IsActive("smashphil.vehicleframework");


        // static VehiclesCompatibility()
        // {
        //     if (VehiclesActive)
        //     {
        //         Log.Message("Armored Up: Attempting to patch Vehicles Framework!");
        //         //VehiclesPatching.DoPatches();
        //         new Harmony("ArmoredUp.VehiclesPatches").PatchCategory("Vehicles");
        //         Log.Message("Armored Up: Successfully Patched Vehicles Framework!");
        //     }
        // }
    }

    public static class VehiclesPatching
    {
        /*public static void DoPatches()
        {
            Type TypeOf_VehicleComponent = AccessTools.TypeByName("Vehicles.VehicleComponent");
            Type TypeOf_VehicleStatHandler = AccessTools.TypeByName("Vehicles.VehicleStatHandler");
            Type VehiclePawn = AccessTools.TypeByName("Vehicles.VehiclePawn");

            if (TypeOf_VehicleComponent == null || TypeOf_VehicleStatHandler == null || VehiclePawn == null)
                return;
            
            MethodInfo ApplyDamageToComponentOriginal = TypeOf_VehicleStatHandler.GetMethod("ApplyDamageToComponent");
            MethodInfo ApplyDamageToComponentPrefix = typeof(VehiclesPatch.VehicleStatHandler_ApplyDamageToComponent_Reroute).GetMethod("Prefix");
            MethodInfo ArmorRatingOriginal = TypeOf_VehicleComponent.GetMethod("ArmorRating");
            MethodInfo ArmorRatingPostfix = typeof(VehiclesPatch.VehicleComponent_ArmorRating_Patch).GetMethod("Postfix");
            MethodInfo TakeDamageOriginal = TypeOf_VehicleComponent.GetMethod("TakeDamage", new[] { VehiclePawn, typeof(DamageInfo).MakeByRefType(), typeof(bool) });
            MethodInfo TakeDamagePrefix = typeof(VehiclesPatch.VehicleComponent_TakeDamage_Patch).GetMethod("Prefix");
            
            Harmony harmony = new Harmony("ArmoredUp.VehiclesPatch");
            harmony.Patch(ApplyDamageToComponentOriginal, prefix: ApplyDamageToComponentPrefix);
            harmony.Patch(ArmorRatingOriginal, prefix: ArmorRatingPostfix);
            harmony.Patch(TakeDamageOriginal, prefix: TakeDamagePrefix);
        }*/
        

    }
}