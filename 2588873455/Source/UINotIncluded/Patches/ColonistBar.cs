using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace UINotIncluded
{
    [HarmonyPatch(typeof(ColonistBar), "CheckRecacheEntries")]
    internal class CheckRecacheEntriesPatch
    {
        public static void Prefix(in bool ___entriesDirty, out bool __state)
        {
            __state = ___entriesDirty;
        }

        public static void Postfix(List<Vector2> ___cachedDrawLocs, bool __state)
        {
            if (__state && Settings.TabsOnTop)
            {
                var copy = new Vector2[___cachedDrawLocs.Count];
                ___cachedDrawLocs.CopyTo(copy);
                ___cachedDrawLocs.Clear();
                foreach (var vector in copy)
                    ___cachedDrawLocs.Add(new Vector2(vector.x, vector.y + UIManager.ExtendedBarHeight));
            }
        }
    }

    [HarmonyPatch(typeof(ColonistBarColonistDrawer), "GroupFrameRect")]
    internal class GroupFrameRectPatch
    {
        public static void Postfix(ref Rect __result)
        {
            if (Settings.TabsOnTop) __result.y += UIManager.ExtendedBarHeight;
        }
    }
}