using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ResistanceRestraintsMod
{
    [StaticConstructorOnStartup]
    public static class TortureBedGizmoRemover
    {
        static TortureBedGizmoRemover()
        {
            var harmony = new Harmony("SilkCircuit.TortureBed.HideGizmos");
            harmony.Patch(
                original: AccessTools.Method(typeof(Building_Bed), "GetGizmos"),
                postfix: new HarmonyMethod(typeof(TortureBedGizmoRemover), nameof(RemoveUnwantedGizmos))
            );
        }

        public static void RemoveUnwantedGizmos(Building_Bed __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.Map == null) return;

            Room bedRoom = __instance.GetRoom();
            if (bedRoom == null) return;

            // Check if a bed from the BuildingsPrisonerRestraints category exists in the same room
            bool hasTortureBed = bedRoom.ContainedThings<Building_Bed>()
                .Any(b => b.def.thingCategories != null && b.def.thingCategories.Contains(ThingCategoryDef.Named("BuildingsPrisonerRestraints")));


            List<Gizmo> gizmos = new List<Gizmo>();
            foreach (var gizmo in __result)
            {
                if (gizmo is Command_Toggle toggle)
                {
                    // Remove the "For Prisoner" toggle if the bed is in the same room as a torture bed
                    if (hasTortureBed && toggle.defaultLabel == "CommandBedSetForPrisonersLabel".Translate())
                    {
                        continue;
                    }



                    // Also remove toggles from any bed in the BuildingsPrisonerRestraints category
                    if (__instance.def.thingCategories != null &&
                        __instance.def.thingCategories.Contains(ThingCategoryDef.Named("BuildingsPrisonerRestraints")) &&
                        (toggle.defaultLabel == "CommandBedSetForPrisonersLabel".Translate() ||
                         toggle.defaultLabel == "CommandBedSetAsMedicalLabel".Translate()))
                    {
                        continue;
                    }

                }
                gizmos.Add(gizmo);
            }
            __result = gizmos;
        }
    }
}
