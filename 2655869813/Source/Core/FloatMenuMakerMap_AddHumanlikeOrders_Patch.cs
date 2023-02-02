using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        private static bool inMenuMaker;

        public static void Prefix(Pawn pawn)
        {
            inMenuMaker = pawn.WorkTagIsDisabled(WorkTags.Violent);
        }

        public static void Postfix()
        {
            inMenuMaker = false;
        }

        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsWeapon), MethodType.Getter)]
        [HarmonyPostfix]
        public static bool IsWeapon(bool __result, ThingDef __instance)
        {
            if (inMenuMaker && BenLubarsVanillaExpandedPatches.nonViolentEquipTools && IsToolDef(__instance))
            {
                return false;
            }

            return __result;
        }

        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsRangedWeapon), MethodType.Getter)]
        [HarmonyPostfix]
        public static bool IsRangedWeapon(bool __result, ThingDef __instance)
        {
            if (inMenuMaker && BenLubarsVanillaExpandedPatches.nonViolentEquipTools && IsToolDef(__instance))
            {
                return false;
            }

            return __result;
        }

        private static readonly string[] ForceToolDefNames = new[]
        {
            "VWE_Gun_FireExtinguisher",
        };

        public static bool IsToolDef(ThingDef thingDef)
        {
            return (thingDef.equippedStatOffsets != null && thingDef.equippedStatOffsets.Count != 0) || ForceToolDefNames.Contains(thingDef.defName);
        }
    }
}
