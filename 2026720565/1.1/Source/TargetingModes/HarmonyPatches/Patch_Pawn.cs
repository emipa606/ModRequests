using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TargetingModes
{

    public static class Patch_Pawn
    {

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.GetGizmos))]
        public static class Patch_GetGizmos
        {

            public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
            {
                __result = ModifiedGizmoEnumerable(__instance, __result);
            }

            private static IEnumerable<Gizmo> ModifiedGizmoEnumerable(Pawn pawn, IEnumerable<Gizmo> result)
            {
                foreach (Gizmo gizmo in result)
                    yield return gizmo;

                if (pawn.IsPlayerControlledAnimal() && typeof(CompTargetingMode).Equals(pawn.TryGetComp<CompTargetingMode>()))
                    foreach (Gizmo gizmo in pawn.TryGetComp<CompTargetingMode>().CompGetGizmosExtra())
                        yield return gizmo;
            }

        }

    }

}
