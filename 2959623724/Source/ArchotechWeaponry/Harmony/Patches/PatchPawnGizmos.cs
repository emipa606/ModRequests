using System.Collections.Generic;
using ArchotechWeaponry.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    public class PatchPawnGizmos
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__result != null)
            {
                foreach (Gizmo g in __result)
                {
                    yield return g; //Return all gizmos from unpatched method before adding ours
                }
            }

            CompArchotechWeapon comp = __instance?.equipment?.Primary?.TryGetComp<CompArchotechWeapon>();
            if (comp != null && __instance.Faction == Faction.OfPlayer)
            {
                foreach (Gizmo g in comp.CompGetGizmosExtra())
                {
                    yield return g;
                }
            }
        }
    }
}