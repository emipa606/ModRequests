using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Who_Next
{
    [StaticConstructorOnStartup]
    public static class ThingSelectionUtility_Patches
    {
        private static bool AttemptSelectNewPawn(int direction)
        {
            bool madeSwitch = false;

            Map map = Find.CurrentMap;
            if (map != null)
            {
                
                Thing selectedThing = Find.Selector.FirstSelectedObject as Thing;
                Corpse selectedCorpse = (selectedThing as Corpse);
                bool corpseSelected = (selectedCorpse != null);
                Pawn selectedPawn = selectedThing as Pawn ?? selectedCorpse?.InnerPawn;

                if (selectedPawn != null)
                {
                    // Prisoners
                    if (selectedPawn.IsPrisonerOfColony)
                        madeSwitch = SelectNextMatchingPawn(map, selectedThing, corpseSelected, p => p.IsPrisonerOfColony, direction);
                    // Animals
                    else if (selectedPawn.AnimalOrWildMan())
                        madeSwitch = SelectNextMatchingPawn(map, selectedThing, corpseSelected, p => p.Faction == selectedPawn.Faction && p.AnimalOrWildMan(), direction);
                    // Peoples
                    else if (selectedPawn.Faction != Faction.OfPlayer)
                        madeSwitch = SelectNextMatchingPawn(map, selectedThing, corpseSelected, p => p.Faction == selectedPawn.Faction && !p.IsPrisonerOfColony && !p.AnimalOrWildMan(), direction);
                }
            }

            return madeSwitch;
        }

        private static bool SelectNextMatchingPawn(Map map, Thing selectedThing, bool corpseSelected, Func<Pawn, bool> filter, int direction = 1)
        {
            //Log.Message("Attempting to select next pawn");

            Func<Thing, bool> pawnAndCorpseFilter = new Func<Thing, bool>(thing =>
            {
                Pawn thingPawn = thing as Pawn;
                if (thing as Pawn != null && (WhoNextMod.settings.allowSwitchingBetweenCorpsesAndLiving || !corpseSelected))
                {
                    return filter(thing as Pawn);
                }
                else
                {
                    Corpse thingCorpse = thing as Corpse;
                    if (thingCorpse != null && thingCorpse.InnerPawn != null && (WhoNextMod.settings.allowSwitchingBetweenCorpsesAndLiving || corpseSelected))
                    {
                        return filter(thingCorpse.InnerPawn);
                    }
                }

                return false;
            });

            List<Thing> pawnsAndCorpsePawns = map.listerThings.AllThings.Where(pawnAndCorpseFilter).ToList();
            
            int selectedIndex = pawnsAndCorpsePawns.FindIndex(thing => thing == selectedThing);
            if (selectedIndex >= 0)
            {
                selectedIndex += direction;
                if (selectedIndex >= pawnsAndCorpsePawns.Count)
                    selectedIndex = 0;
                if (selectedIndex < 0)
                    selectedIndex = pawnsAndCorpsePawns.Count - 1;

                CameraJumper.TryJumpAndSelect(pawnsAndCorpsePawns[selectedIndex]);

                return true;
            }

            return false;
        }

        [HarmonyPatch(typeof(ThingSelectionUtility))]
        [HarmonyPatch("SelectNextColonist", MethodType.Normal)]
        public static class ThingSelectionUtility_SelectNextColonist
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                //Log.Message("ThingSelectionUtility.SelectNextColonist prefix");

                bool callOriginal = !AttemptSelectNewPawn(1);
                return callOriginal;
            }
        }

        [HarmonyPatch(typeof(ThingSelectionUtility))]
        [HarmonyPatch("SelectPreviousColonist", MethodType.Normal)]
        public static class ThingSelectionUtility_SelectPreviousColonist
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                //Log.Message("ThingSelectionUtility.SelectPreviousColonist prefix");

                bool callOriginal = !AttemptSelectNewPawn(-1);
                return callOriginal;
            }
        }
    }
}
