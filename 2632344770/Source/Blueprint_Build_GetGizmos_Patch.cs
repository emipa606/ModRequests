using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FixedIdeoStyle
{
    [HarmonyPatch(typeof(Blueprint_Build), nameof(Blueprint_Build.GetGizmos))]
    static class Blueprint_Build_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Blueprint_Build __instance)
        {
            foreach (var g in __result)
            {
                yield return g;
            }

            var gizmo = FixedIdeoStyle.ChangeStyleGizmo(__instance, __instance.def.entityDefToBuild as ThingDef);
            if (gizmo != null)
            {
                yield return gizmo;
            }
        }
    }
}
