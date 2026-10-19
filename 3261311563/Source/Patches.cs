using HarmonyLib;
using RimWorld;
using Verse;
using static Moonlight.MoonlightUtility;

namespace Moonlight
{
    //This is for events such as eclipse and the sunblocker
    [HarmonyPatch(typeof(GameCondition_NoSunlight), nameof(GameCondition_NoSunlight.SkyTarget))]
	public class Patch_NoSunlight
	{
        static SkyTarget? Postfix(SkyTarget? __result)
		{  
            if (__result.HasValue)
            {
                __result.value.colors.sky *= brightnessMid[day];
                return __result;
            }
            else return null;
        }
    }

    //We're patching here because it's a point of entry with the minimal overhead, unlikely to be bypassed by other mods, and only periodically ticked every 1000th tick.
    [HarmonyPatch(typeof(GoodwillSituationManager), nameof(GoodwillSituationManager.RecalculateAll))]
	public class Patch_GoodwillSituationManager_RecalculateAll
	{
        static void Postfix()
        {
            if (FindMiddle() == 12) UpdateMoonlight(true);
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.FinalizeInit))]
	public class Patch_Map_FinalizeInit
	{
        static void Postfix()
        {
            RefreshCache();
        }
    }
}