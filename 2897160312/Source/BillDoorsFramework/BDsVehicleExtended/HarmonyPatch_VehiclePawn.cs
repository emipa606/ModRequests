using HarmonyLib;
using Vehicles;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(VehiclePawn), "PreApplyDamage")]
    static class PreApplyDamage_reFix
    {
        [HarmonyPrefix]
        static bool Prefix(ref DamageInfo dinfo, VehiclePawn __instance)
        {
            foreach (ThingComp comp in __instance.AllComps)
            {
                if (comp is IVehicleDamageAbsorber shield && shield.AbsorbDamage(ref dinfo))
                {
                    return false;
                }
            }
            return true;
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(VehiclePawn), "TryTakeDamage")]
    static class TryTakeDamage_PreFix
    {
        [HarmonyPrefix]
        static bool Prefix(ref DamageInfo dinfo, VehiclePawn __instance)
        {
            foreach (ThingComp comp in __instance.AllComps)
            {
                if (comp is IVehicleDamageAbsorber shield && shield.AbsorbDamageExplosive(ref dinfo))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
