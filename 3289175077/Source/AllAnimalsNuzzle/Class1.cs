using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace YourModNamespace
{
    [StaticConstructorOnStartup]
    public static class AdjustNuzzleInterval
    {
        static AdjustNuzzleInterval()
        {
            // Get all animals in the game, excluding corpses
            var allAnimalDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.race != null && def.race.Animal && def.thingClass == typeof(Pawn));

            foreach (var animalDef in allAnimalDefs)
            {
                // Check the wildness of the animal
                float wildness = animalDef.race.wildness;
                int nuzzleInterval = GetNuzzleInterval(wildness);

                // Set the nuzzle interval
                animalDef.race.nuzzleMtbHours = nuzzleInterval;
                //Log.Message($"Set nuzzle interval for {animalDef.label} (wildness: {wildness}) to {nuzzleInterval} hours.");
            }

            Log.Message($"[AllAnimalsNuzzle] Loaded");
        }

        private static int GetNuzzleInterval(float wildness)
        {
            // Clamp wildness between 0 and 1
            wildness = Mathf.Clamp(wildness, 0f, 1f);

            // Map wildness to nuzzle interval
            // 0 wildness -> 12 hours, 1 wildness -> 72 hours
            return Mathf.RoundToInt(Mathf.Lerp(12f, 72f, wildness));
        }
    }
}
