using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FixedIdeoStyle
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.GetGizmos))]
    static class Frame_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Frame __instance)
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
