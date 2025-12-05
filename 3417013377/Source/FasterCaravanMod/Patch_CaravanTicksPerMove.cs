using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace FasterCaravanMod
{
    [StaticConstructorOnStartup]
    public static class FasterCaravanMod
    {
        static FasterCaravanMod()
        {
            var harmony = new Harmony("com.example.rimworld.fastercaravanboost");

            // Explicitly define the method to patch
            var original = AccessTools.Method(
                typeof(CaravanTicksPerMoveUtility),
                "GetTicksPerMove",
                new[] { typeof(Caravan), typeof(StringBuilder) }
            );

            var postfix = AccessTools.Method(typeof(Patch_CaravanTicksPerMove), "Postfix");

            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }
    }

    public static class Patch_CaravanTicksPerMove
    {
        public static void Postfix(Caravan caravan, StringBuilder explanation, ref int __result)
        {
            if (caravan == null || caravan.PawnsListForReading == null)
                return;

            // Check if the caravan inventory has enough HikingPoles
            int hikingpolesCount = CountThingInCaravan(caravan, ThingDef.Named("TrekTech_HikingPoles"));
            int hikingbootsCount = CountThingInCaravan(caravan, ThingDef.Named("TrekTech_HikingBoots"));
            int walkietalkiesCount = CountThingInCaravan(caravan, ThingDef.Named("TrekTech_HikingWalkieTalkie"));

            int pawnCount = caravan.PawnsListForReading.Count;

            float totalMultiplier = 1.0f;

            // Add extra blank line if any of the three conditions are met
            if (hikingpolesCount >= pawnCount || hikingbootsCount >= pawnCount || walkietalkiesCount >= pawnCount)
            {
                if (explanation != null)
                {
                    explanation.AppendLine($"");
                }
            }

            // Apply speed boost for HikingPoles
            if (hikingpolesCount >= pawnCount)
            {
                __result = Mathf.RoundToInt(__result * 0.90f); // 10% faster
                totalMultiplier *= 0.90f;
                explanation?.AppendLine();
                explanation?.AppendLine($"Each caravan member is using hiking poles: <color=#00FF00>{11}% faster movement</color>");
            }

            // Apply speed boost for HikingBoots
            if (hikingbootsCount >= pawnCount)
            {
                __result = Mathf.RoundToInt(__result * 0.80f); // 20% faster
                totalMultiplier *= 0.80f;
                explanation?.AppendLine();
                explanation?.AppendLine($"Each caravan member is wearing all-terrain boots: <color=#00FF00>{25}% faster movement</color>");
            }

            // Apply speed boost for WalkieTalkies
            if (walkietalkiesCount >= pawnCount)
            {
                __result = Mathf.RoundToInt(__result * 0.70f); // 30% faster
                totalMultiplier *= 0.70f;
                explanation?.AppendLine();
                explanation?.AppendLine($"Each caravan member is holding a walkie-talkie: <color=#00FF00>{43}% faster movement</color>");
            }

            // Calculate total speed multiplier
            float speedMultiplier = 1 / totalMultiplier;

            // Append the total explanation
            explanation?.AppendLine();
            explanation?.AppendLine($"Effective travel speed increase: <color=#00FF00>{(speedMultiplier - 1) * 100:F1}%</color>");
        }


        private static int CountThingInCaravan(Caravan caravan, ThingDef thingDef)
        {
            int count = 0;
            List<Pawn> pawns = caravan.PawnsListForReading;
            foreach (var pawn in pawns)
            {
                if (pawn.inventory != null)
                {
                    foreach (var thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def == thingDef)
                            count += thing.stackCount;
                    }
                }
            }
            return count;
        }
    }
}
