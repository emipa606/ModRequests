using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch(typeof(PlantUtility), nameof(PlantUtility.CanSowOnGrower))]
    static class PlantUtility_CanSowOnGrower_Patch
	{
		public static bool Postfix(bool __result, ThingDef plantDef, object obj)
		{
			if (BenLubarsVanillaExpandedPatches.checkPlanterFertility && obj is Building_PlantGrower planter)
            {
                if (planter.def.fertility >= 0f && planter.def.fertility < plantDef.plant.fertilityMin)
                {
                    return false;
                }
            }

            return __result;
		}
    }
}
