using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WoodIsNaturalForTrees;

internal static class HarmonyPatches
{
    [HarmonyPatch(typeof(MeditationUtility), nameof(MeditationUtility.CountsAsArtificialBuilding), typeof(Thing))]
    private static class PatchCountsAsArtificialBuildingThing
    {
        private static void Postfix(Thing t, ref bool __result)
        {
            if (
                // No point checking for more stuff if it's not artificial already
                !__result ||
                // Make sure that the thing actually has any stuff
                t.Stuff?.stuffProps?.categories == null ||
                // Check if we have any stuff that's allowed at all first
                !WoodIsNaturalForTreesMod.settings.stuffMadeFrom.Any() ||
                // Make sure what we're placing isn't a nature shrine or similar thing, as it'll end up not being affected by artificial buildings until reload
                !t.def.building.artificialForMeditationPurposes)
                return;

            if (t.Stuff.stuffProps.categories.All(c => WoodIsNaturalForTreesMod.settings.stuffMadeFrom.Contains(c)))
                __result = false;
        }
    }

    [HarmonyPatch(typeof(MeditationUtility), nameof(MeditationUtility.CountsAsArtificialBuilding), typeof(ThingDef), typeof(Faction))]
    private static class PatchCountsAsArtificialBuildingDef
    {
        private static void Postfix(ThingDef def, ref bool __result)
        {
            if (
                // No point checking for more stuff if it's not artificial already
                !__result ||
                // Check if we have any stuff that's allowed at all first
                !WoodIsNaturalForTreesMod.settings.stuffMadeFrom.Any() ||
                Find.DesignatorManager.SelectedDesignator is not Designator_Place place ||
                // Make sure that the thing actually has any stuff
                place.StuffDef?.stuffProps?.categories == null ||
                // Make sure what we're placing isn't a nature shrine or similar thing, as it'll end up not being affected by artificial buildings until reload
                (place.PlacingDef is ThingDef thingDef && !thingDef.building.artificialForMeditationPurposes))
                return;

            if (place.StuffDef.stuffProps.categories.All(c => WoodIsNaturalForTreesMod.settings.stuffMadeFrom.Contains(c)))
                __result = false;
        }
    }
}