using ArchotechWeaponry.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(Verb_Shoot))]
    [HarmonyPatch("TryCastShot")]
    public class PatchVerbShootTryCastShot
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, Verb_Shoot __instance)
        {
            if (__instance.EquipmentSource is ThingWithComps thing
                && thing.GetComp<CompLimitedUsage>() is CompLimitedUsage comp
                && comp.currentCharges <= 0)
            {
                __result = false;
                MoteMaker.ThrowText(__instance.caster.DrawPos, __instance.caster.Map, "Depleted !");
                return false;
            }
            else
            {
                return true;
            }
        }
        
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Verb_Shoot __instance)
        {
            if (__instance.EquipmentSource is ThingWithComps thing &&
                thing.GetComp<CompLimitedUsage>() is CompLimitedUsage comp)
            {
                comp.ConsumeCharge();
            }
        }
    }
}