using HarmonyLib;
using VCE_Fishing;
using VCE_Fishing.Options;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Fishing
{
    [HarmonyPatch(typeof(Zone_Fishing), nameof(Zone_Fishing.GetInspectString))]
    static class Zone_Fishing_GetInspectString_Patch
    {
        public static void Postfix(ref string __result, Zone_Fishing __instance)
        {
            if (!BenLubarsVanillaExpandedPatches.showFishingZoneCellCount)
            {
                return;
            }

            var size = __instance.Cells.Count;
            bool actuallyBigEnough = size >= VCE_Fishing_Settings.VCEF_minimumZoneSize;

            if (__instance.isZoneBigEnough != actuallyBigEnough)
            {
                __result = "BenLubarsVanillaExpandedPatches_FishingZone_Desync".Translate();
            }

            __result += "\n" + "BenLubarsVanillaExpandedPatches_FishingZone_Size".Translate(size.Named("CUR"), VCE_Fishing_Settings.VCEF_minimumZoneSize.Named("REQ"));
        }
    }
}
