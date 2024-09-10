using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WeaponEquipment
{
    [StaticConstructorOnStartup]
    public static class Class1
    {
        static Class1()
        {
            new Harmony("WeaponEquipment.Mod").PatchAll();
        }
    }
    public class WeaponEquipmentOptions : DefModExtension
    {
        public ThingDef apparelToWearAfterEquipping;
        public ThingDef apparelToDestroyAfterDropping;
        public ThingDef stuffForApparel;
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
    public static class AddEquipment_Patch
    {
        private static void Prefix(Pawn_EquipmentTracker __instance, ref ThingWithComps newEq)
        {
            var weaponOptions = newEq.def.GetModExtension<WeaponEquipmentOptions>();
            if (weaponOptions != null && __instance.pawn.apparel != null)
            {
                var apparel = ThingMaker.MakeThing(weaponOptions.apparelToWearAfterEquipping, weaponOptions.stuffForApparel) as Apparel;
                __instance.pawn.apparel.Wear(apparel);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
    public static class TryDropEquipment_Patch
    {
        private static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
        {
            var weaponOptions = resultingEq?.def.GetModExtension<WeaponEquipmentOptions>();
            if (weaponOptions != null && weaponOptions.apparelToDestroyAfterDropping != null)
            {
                var apparel = __instance.pawn.apparel?.WornApparel.FirstOrDefault(x => x.def == weaponOptions.apparelToDestroyAfterDropping);
                if (apparel != null)
                {
                    __instance.pawn.apparel.TryDrop(apparel, out var resultingAp);
                    resultingAp.Destroy();
                }
            }
        }
    }
}
