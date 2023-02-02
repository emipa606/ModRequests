using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ReTill
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.GetGizmos))]
    static class Zone_Growing_GetGizmos_Patch
    {
        private static readonly Texture2D VCE_Plow = ContentFinder<Texture2D>.Get("UI/VCE_Plow", reportFailure: false);

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Zone_Growing __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }

            if (!ReTill.VanillaPlantsExpandedInstalled)
            {
                yield break;
            }

            bool isActive = AutoReTillZone.Get(__instance);
            yield return new Command_Toggle
            {
                isActive = () => isActive,
                toggleAction = delegate
                {
                    foreach (var zone in Find.Selector.SelectedObjects.OfType<Zone_Growing>())
                    {
                        AutoReTillZone.Set(zone, !isActive);
                    }
                },
                icon = VCE_Plow,
                defaultLabel = isActive ? "ReTill_AutoReTillZone_On".Translate() : "ReTill_AutoReTillZone_Off".Translate(),
                defaultDesc = isActive ? "ReTill_AutoReTillZone_On_Desc".Translate() : "ReTill_AutoReTillZone_Off_Desc".Translate(),
            };
        }
    }
}
