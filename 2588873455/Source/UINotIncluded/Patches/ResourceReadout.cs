using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace UINotIncluded
{
    [HarmonyPatch(typeof(ResourceReadout), "DoReadoutSimple")]
    internal class DoReadoutSimplePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(ref Rect rect)
        {
            rect.y += UIManager.ResourceGap;
        }
    }

    [HarmonyPatch(typeof(ResourceReadout), "DoReadoutCategorized")]
    internal class DoReadoutCategorizedPatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(ref Rect rect)
        {
            rect.y += UIManager.ResourceGap;
        }
    }
}