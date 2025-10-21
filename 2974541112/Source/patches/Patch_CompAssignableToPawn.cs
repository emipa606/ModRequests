using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(CompAssignableToPawn_Bed), "IdeoligionForbids")]
public static class Patch_IdeoligionForbids
{
    public static bool Prefix(CompAssignableToPawn_Bed __instance, ref bool __result)
    {
        if (__instance.parent is Building_Base)
        {
            __result = false;
            return false;
        }
        return true;
    }
}

