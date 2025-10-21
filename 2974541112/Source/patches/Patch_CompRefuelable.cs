using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// TSS: hide CompRefuelable "don't refuel" overlay
[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.PostDraw))]
public static class Patch_CompRefuelable
{
    public static bool Prefix(CompRefuelable __instance)
    {
        if (__instance.parent is Building_TSS)
        {
            return false;
        }
        return true;
    }
}

